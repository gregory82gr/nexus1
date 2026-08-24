// Drill catalogue -- authored curriculum content, by design invented
// (Ch. 9's own thesis: this is the one screen where inventing the
// numbers is the point). Numbers taken from the live demo's own Training
// Mode page (the book's prose paraphrases the same five drills slightly
// less precisely), not fabricated for this port.
export type DrillKind = 'hold' | 'follow' | 'trip';
export type Difficulty = 'CORE' | 'INTERMEDIATE' | 'ADVANCED';

export interface ScheduleStep {
  atSeconds: number;
  targetPercent: number;
}

export interface DrillDefinition {
  id: string;
  name: string;
  difficulty: Difficulty;
  description: string;
  kind: DrillKind;
  initialPowerPercent: number;
  timeLimitSeconds: number;
  // 'hold'
  targetPercent?: number;
  tolerancePercent?: number;
  holdSecondsRequired?: number;
  undershootFloorPercent?: number;
  // 'follow'
  schedule?: ScheduleStep[];
  totalSeconds?: number;
  // 'trip'
  cueAtSeconds?: number;
  reactionWindowSeconds?: number;
}

export const DRILLS: readonly DrillDefinition[] = [
  {
    id: 'power-maneuver',
    name: 'Power Maneuver',
    difficulty: 'CORE',
    description: '100% → 80%, hold ±2% for 25 s',
    kind: 'hold',
    initialPowerPercent: 100,
    targetPercent: 80,
    tolerancePercent: 2,
    holdSecondsRequired: 25,
    timeLimitSeconds: 90,
  },
  {
    id: 'deep-power-reduction',
    name: 'Deep Power Reduction',
    difficulty: 'INTERMEDIATE',
    description: '100% → 50%, hold ±3% for 20 s, no undershoot < 40%',
    kind: 'hold',
    initialPowerPercent: 100,
    targetPercent: 50,
    tolerancePercent: 3,
    holdSecondsRequired: 20,
    undershootFloorPercent: 40,
    timeLimitSeconds: 90,
  },
  {
    id: 'reactor-trip-response',
    name: 'Reactor Trip Response',
    difficulty: 'CORE',
    description: 'SCRAM on the trip cue — scored on reaction time',
    kind: 'trip',
    initialPowerPercent: 100,
    cueAtSeconds: 10,
    reactionWindowSeconds: 8,
    timeLimitSeconds: 30,
  },
  {
    id: 'xenon-transient-mgmt',
    name: 'Xenon Transient Mgmt',
    difficulty: 'ADVANCED',
    description:
      '100% → 60%, hold ±3% as Xe-135 peaks (run at 600×) — simplified: no continuous xenon term is modeled here (see core/physics/point-kinetics.ts); this drill exercises the same hold-in-band mechanic at high time-acceleration, not a real poison transient',
    kind: 'hold',
    initialPowerPercent: 100,
    targetPercent: 60,
    tolerancePercent: 3,
    holdSecondsRequired: 30,
    timeLimitSeconds: 120,
  },
  {
    id: 'load-follow-grid',
    name: 'Load-Follow (Grid Demand)',
    difficulty: 'INTERMEDIATE',
    description: 'Track grid demand 100 → 85 → 95 → 80%, within ±3%',
    kind: 'follow',
    initialPowerPercent: 100,
    tolerancePercent: 3,
    schedule: [
      { atSeconds: 0, targetPercent: 100 },
      { atSeconds: 20, targetPercent: 85 },
      { atSeconds: 40, targetPercent: 95 },
      { atSeconds: 60, targetPercent: 80 },
    ],
    totalSeconds: 80,
    timeLimitSeconds: 80,
  },
];
