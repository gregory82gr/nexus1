import { TwinScene } from './twin-scene';

// Ch. 8's own third spec is "the one that pays for itself": it counts
// live geometries and materials against a fake three.js and asserts they
// reach zero, so a contributor who adds a mesh and forgets to include it
// in the disposal traversal gets a failing test rather than a GPU leak
// that surfaces weeks later as "the 3D page stops working eventually."
// This mirrors that pattern for TwinScene.
interface Counters {
  liveGeometries: number;
  liveMaterials: number;
}

class Object3DFake {
  children: Object3DFake[] = [];
  position = { set: jest.fn() };
  rotation = { x: 0, y: 0 };
  add(child: Object3DFake): void {
    this.children.push(child);
  }
}

class SceneFake extends Object3DFake {
  traverse(cb: (obj: Object3DFake) => void): void {
    const walk = (obj: Object3DFake): void => {
      cb(obj);
      obj.children.forEach(walk);
    };
    walk(this);
  }
}

function makeFakeThree(counters: Counters) {
  class GeometryFake {
    constructor() {
      counters.liveGeometries++;
    }
    dispose = jest.fn(() => {
      counters.liveGeometries--;
    });
  }

  class MaterialFake {
    color = { setHex: jest.fn() };
    opacity = 0;
    emissive: unknown;
    emissiveIntensity = 0;
    constructor() {
      counters.liveMaterials++;
    }
    dispose = jest.fn(() => {
      counters.liveMaterials--;
    });
  }

  class MeshFake extends Object3DFake {
    constructor(
      public geometry: GeometryFake,
      public material: MaterialFake,
    ) {
      super();
    }
  }

  class PerspectiveCameraFake {
    aspect = 1;
    position = { set: jest.fn() };
    updateProjectionMatrix = jest.fn();
  }

  class WebGLRendererFake {
    domElement = document.createElement('canvas');
    setPixelRatio = jest.fn();
    setSize = jest.fn();
    render = jest.fn();
    dispose = jest.fn();
    forceContextLoss = jest.fn();
  }

  return {
    Scene: SceneFake,
    Group: Object3DFake,
    AmbientLight: Object3DFake,
    DirectionalLight: Object3DFake,
    IcosahedronGeometry: GeometryFake,
    MeshStandardMaterial: MaterialFake,
    Mesh: MeshFake,
    PerspectiveCamera: PerspectiveCameraFake,
    WebGLRenderer: WebGLRendererFake,
    Color: class {
      constructor(public hex?: number) {}
    },
  } as unknown as typeof import('three');
}

function makeHost(): HTMLDivElement {
  const host = document.createElement('div');
  Object.defineProperty(host, 'clientWidth', { value: 400, configurable: true });
  Object.defineProperty(host, 'clientHeight', { value: 300, configurable: true });
  document.body.appendChild(host);
  return host;
}

describe('TwinScene', () => {
  it('cancels its frame loop and disposes every geometry and material on destroy', () => {
    const counters: Counters = { liveGeometries: 0, liveMaterials: 0 };
    const three = makeFakeThree(counters);
    const host = makeHost();
    const cancelSpy = jest.spyOn(window, 'cancelAnimationFrame');

    const scene = new TwinScene(three, host);
    expect(counters.liveGeometries).toBe(1);
    expect(counters.liveMaterials).toBe(1);
    expect(host.children.length).toBe(1); // the canvas, appended by the constructor

    scene.start();
    scene.destroy();

    expect(cancelSpy).toHaveBeenCalledTimes(1);
    expect(counters.liveGeometries).toBe(0);
    expect(counters.liveMaterials).toBe(0);
    expect(host.children.length).toBe(0);

    document.body.removeChild(host);
    cancelSpy.mockRestore();
  });

  it('does not throw disposing a scene that was never started', () => {
    const counters: Counters = { liveGeometries: 0, liveMaterials: 0 };
    const three = makeFakeThree(counters);
    const host = makeHost();

    const scene = new TwinScene(three, host);
    expect(() => scene.destroy()).not.toThrow();
    expect(counters.liveGeometries).toBe(0);
    expect(counters.liveMaterials).toBe(0);

    document.body.removeChild(host);
  });

  it('accepts a null state as a neutral fallback without throwing', () => {
    const counters: Counters = { liveGeometries: 0, liveMaterials: 0 };
    const three = makeFakeThree(counters);
    const host = makeHost();

    const scene = new TwinScene(three, host);
    expect(() => scene.setState(null)).not.toThrow();
    expect(() => scene.setState({ color: 0x3ddc84, opacity: 0.8 })).not.toThrow();

    scene.destroy();
    document.body.removeChild(host);
  });
});
