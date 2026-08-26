// Unlike severity-tone.ts's own free-text keyword matching (this
// codebase seeds different severity vocabularies per context), Status
// here is the real ASP.NET Core HealthStatus enum's own ToString() --
// exactly three possible values, never anything else. An exact match,
// not a guess.
export type StatusTone = 'ok' | 'warn' | 'crit' | 'unknown';

export function statusTone(status: string): StatusTone {
  switch (status) {
    case 'Healthy':
      return 'ok';
    case 'Degraded':
      return 'warn';
    case 'Unhealthy':
      return 'crit';
    default:
      return 'unknown';
  }
}
