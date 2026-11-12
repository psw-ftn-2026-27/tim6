import { ComponentFixture, TestBed } from '@angular/core/testing';
import { GeographicMap, MapCoordinate } from './geographic-map';

vi.hoisted(() => {
  Object.defineProperty(SVGSVGElement.prototype, 'createSVGRect', {
    configurable: true,
    value: () => ({}),
  });
});

describe('GeographicMap', () => {
  let fixture: ComponentFixture<GeographicMap>;

  beforeAll(() => {
    vi.spyOn(HTMLElement.prototype, 'clientWidth', 'get').mockReturnValue(800);
    vi.spyOn(HTMLElement.prototype, 'clientHeight', 'get').mockReturnValue(400);
    vi.spyOn(HTMLElement.prototype, 'offsetWidth', 'get').mockReturnValue(800);
    vi.spyOn(HTMLElement.prototype, 'offsetHeight', 'get').mockReturnValue(400);
    vi.spyOn(HTMLElement.prototype, 'getBoundingClientRect').mockReturnValue({
      x: 0,
      y: 0,
      top: 0,
      left: 0,
      right: 800,
      bottom: 400,
      width: 800,
      height: 400,
      toJSON: () => ({}),
    });
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [GeographicMap] }).compileComponents();
    fixture = TestBed.createComponent(GeographicMap);
  });

  afterEach(() => {
    fixture.destroy();
  });

  afterAll(() => {
    vi.restoreAllMocks();
  });

  it('displays every pin without a path when no path is provided', () => {
    fixture.componentRef.setInput('pins', [
      { latitude: 45.2671, longitude: 19.8335 },
      { latitude: 44.8125, longitude: 20.4612 },
    ]);

    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('.geographic-map__pin-icon')).toHaveLength(2);
    expect(fixture.nativeElement.querySelector('.geographic-map__path')).toBeNull();
  });

  it('displays a provided path and removes only the path when it is cleared', () => {
    fixture.componentRef.setInput('pins', [{ latitude: 45, longitude: 19 }]);
    fixture.componentRef.setInput('path', [
      { latitude: 45, longitude: 19 },
      { latitude: 45.1, longitude: 19.1 },
    ]);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.geographic-map__path')).not.toBeNull();

    fixture.componentRef.setInput('path', null);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.geographic-map__path')).toBeNull();
    expect(fixture.nativeElement.querySelectorAll('.geographic-map__pin-icon')).toHaveLength(1);
  });

  it('emits path length when the path changes and null when it is removed', () => {
    const lengths: (number | null)[] = [];
    fixture.componentInstance.pathLengthKm.subscribe((length) => lengths.push(length));
    fixture.componentRef.setInput('path', equatorPath(1));
    fixture.detectChanges();

    expect(lengths[0]).toBeCloseTo(111.2, 0);

    fixture.componentRef.setInput('path', equatorPath(2));
    fixture.detectChanges();
    expect(lengths[1]).toBeCloseTo(222.4, 0);

    fixture.componentRef.setInput('path', null);
    fixture.detectChanges();
    expect(lengths[2]).toBeNull();
  });

  it('keeps one selected marker and emits new coordinates after every enabled click', () => {
    const selected: MapCoordinate[] = [];
    fixture.componentInstance.locationSelected.subscribe((coordinate) => selected.push(coordinate));
    fixture.componentRef.setInput('selectionEnabled', true);
    fixture.detectChanges();

    clickMap(fixture, 400, 200);
    clickMap(fixture, 500, 200);

    expect(selected).toHaveLength(2);
    expect(selected[1]).not.toEqual(selected[0]);
    expect(fixture.nativeElement.querySelectorAll('.geographic-map__selected-icon')).toHaveLength(1);
  });

  it('does not select or emit a location when selection is disabled', () => {
    const selected: MapCoordinate[] = [];
    fixture.componentInstance.locationSelected.subscribe((coordinate) => selected.push(coordinate));
    fixture.detectChanges();

    clickMap(fixture, 400, 200);

    expect(selected).toHaveLength(0);
    expect(fixture.nativeElement.querySelector('.geographic-map__selected-icon')).toBeNull();
  });
});

function equatorPath(longitude: number): MapCoordinate[] {
  return [
    { latitude: 0, longitude: 0 },
    { latitude: 0, longitude },
  ];
}

function clickMap(fixture: ComponentFixture<GeographicMap>, x: number, y: number): void {
  const event = new MouseEvent('click', {
    bubbles: true,
    clientX: x,
    clientY: y,
  });
  fixture.nativeElement.dispatchEvent(event);
  fixture.detectChanges();
}
