import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class NetworkStatusService {
  public isOnline = signal<boolean>(typeof navigator !== 'undefined' ? navigator.onLine : true);
  public isLowBandwidth = signal<boolean>(false);
  public connectionType = signal<string>('unknown');

  constructor() {
    if (typeof window !== 'undefined') {
      window.addEventListener('online', () => this.updateOnlineStatus(true));
      window.addEventListener('offline', () => this.updateOnlineStatus(false));

      this.checkNetworkQuality();
    }
  }

  private updateOnlineStatus(online: boolean): void {
    this.isOnline.set(online);
    if (online) {
      this.checkNetworkQuality();
    } else {
      this.isLowBandwidth.set(true);
      this.connectionType.set('offline');
    }
  }

  public checkNetworkQuality(): void {
    const nav = navigator as unknown as {
      connection?: { effectiveType?: string; downlink?: number; rtt?: number };
    };
    if (nav.connection) {
      const conn = nav.connection;
      const type = conn.effectiveType || 'unknown';
      this.connectionType.set(type);

      // Si es 2g, slow-2g o el downlink es menor a 0.5 Mbps
      const isSlow =
        type === 'slow-2g' ||
        type === '2g' ||
        (typeof conn.downlink === 'number' && conn.downlink < 0.8);
      this.isLowBandwidth.set(isSlow);
    }
  }

  private memoryStorage = new Map<string, string>();

  private getStorage(): { getItem(k: string): string | null; setItem(k: string, v: string): void; removeItem(k: string): void } {
    try {
      if (typeof window !== 'undefined' && window.localStorage && typeof window.localStorage.getItem === 'function') {
        return window.localStorage;
      }
    } catch {
      // Fallback a memoria
    }
    return {
      getItem: (k: string) => this.memoryStorage.get(k) ?? null,
      setItem: (k: string, v: string) => { this.memoryStorage.set(k, v); },
      removeItem: (k: string) => { this.memoryStorage.delete(k); },
    };
  }

  // Cache helper con TTL para conexiones lentas u offline
  public setCachedData<T>(key: string, data: T, ttlMinutes = 30): void {
    try {
      const record = {
        value: data,
        expiresAt: Date.now() + ttlMinutes * 60 * 1000,
      };
      this.getStorage().setItem(`titulacion_cache_${key}`, JSON.stringify(record));
    } catch {
      // Ignorar quota exceeded
    }
  }

  public getCachedData<T>(key: string): T | null {
    try {
      const itemStr = this.getStorage().getItem(`titulacion_cache_${key}`);
      if (!itemStr) return null;
      const record = JSON.parse(itemStr);
      if (Date.now() > record.expiresAt) {
        this.getStorage().removeItem(`titulacion_cache_${key}`);
        return null;
      }
      return record.value as T;
    } catch {
      return null;
    }
  }
}
