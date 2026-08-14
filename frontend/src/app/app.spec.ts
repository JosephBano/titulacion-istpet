import { TestBed } from '@angular/core/testing';
import { BrowserTestingModule, platformBrowserTesting } from '@angular/platform-browser/testing';
import { describe, expect, it } from 'vitest';
import { App } from './app';

try {
  TestBed.initTestEnvironment(BrowserTestingModule, platformBrowserTesting());
} catch {
  // Entorno ya inicializado
}

describe('App', () => {
  it('se crea la instancia de la aplicación', () => {
    const app = new App();
    expect(app).toBeDefined();
  });
});
