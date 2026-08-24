// Authored reference content -- genuinely correct, general nondestructive-
// testing domain knowledge, not plant telemetry and not fabricated. The
// book's own Ch. 16 treats this exact table the same way: "None of it is
// plant data. It is authored reference content, correct, and it needs no
// provenance marker beyond an author and a review date, because a reader
// cannot mistake a methods table for a measurement." Same class as
// Model Analysis's model-constants panel or Training Mode's SCORING
// table -- reference material, clearly labeled as such, not a claim about
// any specific unit or asset.
export interface NdtMethodReference {
  code: string;
  name: string;
  detects: string;
  typicalUse: string;
  note?: string;
}

export const NDT_METHODS: readonly NdtMethodReference[] = [
  {
    code: 'RT',
    name: 'Radiography',
    detects: 'Internal voids, inclusions, porosity',
    typicalUse: 'Film or digital detector; the classic "film" inspection.',
  },
  {
    code: 'UT',
    name: 'Ultrasonic',
    detects: 'Internal and surface-breaking cracks, thickness loss',
    typicalUse: 'Pulsed transducer coupled to the component surface.',
  },
  {
    code: 'VT',
    name: 'Visual',
    detects: 'Surface indications, corrosion, wear, deformation',
    typicalUse: 'Direct or remote (borescope/camera) visual examination.',
  },
  {
    code: 'ECT',
    name: 'Eddy Current',
    detects: 'Surface and near-surface cracks in conductive material',
    typicalUse: 'Induced-current probe; works on any conductor, magnetic or not.',
  },
  {
    code: 'PT',
    name: 'Dye Penetrant',
    detects: 'Surface-breaking cracks and porosity',
    typicalUse: 'Penetrant liquid + developer; needs a clean, accessible surface.',
  },
  {
    code: 'MT',
    name: 'Magnetic Particle',
    detects: 'Surface and near-surface cracks in ferromagnetic material only',
    typicalUse: 'Magnetized part + iron particles; indications form at flux leakage.',
    note: 'Generally N/A for control-rod cladding: zirconium alloy and austenitic stainless steel are both non-ferromagnetic, so this method cannot be used on the rods themselves — a real physical constraint, not a missing result.',
  },
];
