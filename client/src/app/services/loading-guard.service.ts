import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LoadingGuardService {
  private static activeRequests = new Set<string>();

  static isRequestActive(key: string): boolean {
    return this.activeRequests.has(key);
  }

  static startRequest(key: string): boolean {
    if (this.activeRequests.has(key)) {
      return false; // Request already active
    }
    this.activeRequests.add(key);
    return true; // Can proceed
  }

  static endRequest(key: string): void {
    this.activeRequests.delete(key);
  }

  static clearAll(): void {
    this.activeRequests.clear();
  }
}
