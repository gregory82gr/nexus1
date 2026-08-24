import type * as THREE from 'three';

// The non-Angular half of Ch. 8's ownership split: Angular owns a <div>,
// the toolbar, and the lifecycle hook that calls destroy(); this class
// owns everything inside that div -- the canvas, the WebGL context, the
// scene graph, and the render loop -- and knows nothing about Angular.
//
// Renders one abstract object, not a plant. See plant-3d.ts's own doc
// comment for why Ch. 8's physical plant-layout scene (containments,
// turbines, per-class steam-generator counts) isn't ported here: none of
// its inputs exist in the real per-unit twin-state endpoint this screen
// is wired to.
export interface TwinVisualState {
  color: number;
  opacity: number;
}

export class TwinScene {
  private readonly scene: THREE.Scene;
  private readonly camera: THREE.PerspectiveCamera;
  private readonly renderer: THREE.WebGLRenderer;
  private readonly group: THREE.Group;
  private readonly mesh: THREE.Mesh;

  private frameId: number | null = null;
  private readonly resizeObserver: ResizeObserver;

  // Drag-to-rotate -- the one interaction convention carried over from
  // the live demo's own 3D screens ("Drag a core to rotate" / "Drag the
  // scene to rotate"), not the demo's physical plant content.
  private dragging = false;
  private lastX = 0;
  private lastY = 0;
  private readonly onPointerDown = (e: PointerEvent): void => {
    this.dragging = true;
    this.lastX = e.clientX;
    this.lastY = e.clientY;
  };
  private readonly onPointerUp = (): void => {
    this.dragging = false;
  };
  private readonly onPointerMove = (e: PointerEvent): void => {
    if (!this.dragging) return;
    const dx = e.clientX - this.lastX;
    const dy = e.clientY - this.lastY;
    this.lastX = e.clientX;
    this.lastY = e.clientY;
    this.group.rotation.y += dx * 0.01;
    this.group.rotation.x += dy * 0.01;
  };

  constructor(
    private readonly three: typeof THREE,
    private readonly host: HTMLElement,
  ) {
    this.scene = new three.Scene();
    this.camera = new three.PerspectiveCamera(45, 1, 0.1, 100);
    this.camera.position.set(0, 1.2, 5);

    this.renderer = new three.WebGLRenderer({ antialias: true, alpha: true });
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    host.appendChild(this.renderer.domElement);

    this.scene.add(new three.AmbientLight(0xffffff, 0.5));
    const key = new three.DirectionalLight(0xffffff, 0.9);
    key.position.set(3, 4, 5);
    this.scene.add(key);

    this.group = new three.Group();
    this.scene.add(this.group);

    const geometry = new three.IcosahedronGeometry(1.2, 1);
    const material = new three.MeshStandardMaterial({
      color: 0x3c5257,
      transparent: true,
      opacity: 0.35,
      roughness: 0.4,
      metalness: 0.1,
    });
    this.mesh = new three.Mesh(geometry, material);
    this.group.add(this.mesh);

    this.resizeObserver = new ResizeObserver(() => this.resize());
    this.resizeObserver.observe(host);
    this.resize();

    host.addEventListener('pointerdown', this.onPointerDown);
    window.addEventListener('pointerup', this.onPointerUp);
    window.addEventListener('pointermove', this.onPointerMove);
  }

  start(): void {
    const loop = (): void => {
      this.group.rotation.y += 0.0015; // slow idle spin
      this.renderer.render(this.scene, this.camera);
      this.frameId = requestAnimationFrame(loop);
    };
    this.frameId = requestAnimationFrame(loop);
  }

  // The only state allowed to cross into the scene (Ch. 8's own "three
  // things allowed to cross" discipline, narrowed to one here since this
  // screen has no selection and no toggles yet).
  setState(state: TwinVisualState | null): void {
    const material = this.mesh.material as THREE.MeshStandardMaterial;
    const color = state?.color ?? 0x3c5257;
    material.color.setHex(color);
    material.opacity = state?.opacity ?? 0.2;
    material.emissive = new this.three.Color(color);
    material.emissiveIntensity = (state?.opacity ?? 0.2) * 0.3;
  }

  private resize(): void {
    const { clientWidth, clientHeight } = this.host;
    if (clientWidth === 0 || clientHeight === 0) return;
    this.camera.aspect = clientWidth / clientHeight;
    this.camera.updateProjectionMatrix();
    this.renderer.setSize(clientWidth, clientHeight);
  }

  destroy(): void {
    // 1. Stop the loop first -- disposing under a live frame is a crash
    //    (Ch. 8's own teardown ordering).
    if (this.frameId !== null) {
      cancelAnimationFrame(this.frameId);
      this.frameId = null;
    }

    // 2. Detach listeners.
    this.resizeObserver.disconnect();
    this.host.removeEventListener('pointerdown', this.onPointerDown);
    window.removeEventListener('pointerup', this.onPointerUp);
    window.removeEventListener('pointermove', this.onPointerMove);

    // 3. Walk the graph -- geometries and materials are separate GPU
    //    allocations; freeing one does not free the other.
    this.scene.traverse((obj) => {
      const mesh = obj as THREE.Mesh;
      mesh.geometry?.dispose?.();
      const mats = Array.isArray(mesh.material) ? mesh.material : mesh.material ? [mesh.material] : [];
      mats.forEach((m) => m?.dispose?.());
    });

    // 4. The renderer last, and force the context to be released.
    this.renderer.dispose();
    this.renderer.forceContextLoss();
    if (this.renderer.domElement.parentNode === this.host) {
      this.host.removeChild(this.renderer.domElement);
    }
  }
}
