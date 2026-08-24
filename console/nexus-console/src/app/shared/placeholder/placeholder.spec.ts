import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PlaceholderComponent } from './placeholder';

describe('PlaceholderComponent', () => {
  let fixture: ComponentFixture<PlaceholderComponent>;
  const pill = () => fixture.nativeElement.querySelector('.pill') as HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlaceholderComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(PlaceholderComponent);
    fixture.componentRef.setInput('title', 'Plant 3D View');
    fixture.componentRef.setInput('chapter', 27);
  });

  // the spec that forces the difference
  it('renders the stub state by default', () => {
    fixture.componentRef.setInput('failed', false);
    fixture.detectChanges();
    expect(pill().textContent).toContain('NOT YET BUILT');
    expect(pill().classList).toContain('info');
  });

  it('renders the unreachable state when the source is down', () => {
    fixture.componentRef.setInput('failed', true);
    fixture.detectChanges();
    expect(pill().textContent).toContain('UNREACHABLE');
    expect(pill().classList).toContain('crit');
  });
});
