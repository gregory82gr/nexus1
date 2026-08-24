// Jest + jest-preset-angular (Ch. 6 onward), not Karma — a named departure
// from the Angular CLI's historical default, per the book's own decision.
module.exports = {
  preset: 'jest-preset-angular',
  setupFilesAfterEnv: ['<rootDir>/setup-jest.ts'],
};
