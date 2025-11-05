import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap, map } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken?: string;
  user?: {
    id: string;
    email: string;
    name?: string;
  };
}

export interface RegisterResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly platformId = inject(PLATFORM_ID);
  private readonly isBrowser: boolean;
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private readonly TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';

  private isAuthenticatedSubject: BehaviorSubject<boolean>;
  public isAuthenticated$: Observable<boolean>;

  constructor() {
    this.isBrowser = isPlatformBrowser(this.platformId);
    this.isAuthenticatedSubject = new BehaviorSubject<boolean>(this.hasToken());
    this.isAuthenticated$ = this.isAuthenticatedSubject.asObservable();
  }

  register(credentials: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<ApiResponse<RegisterResponse>>(`${this.apiUrl}/register`, credentials).pipe(
      map((response) => response.data!)
    );
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<ApiResponse<LoginResponse>>(`${this.apiUrl}/login`, credentials).pipe(
      tap((response) => {
        if (response.success && response.data) {
          this.storeToken(response.data.accessToken);
          if (response.data.refreshToken) {
            this.storeRefreshToken(response.data.refreshToken);
          }
          this.isAuthenticatedSubject.next(true);
        }
      }),
      map((response) => response.data!)
    );
  }

  logout(): void {
    if (this.isBrowser) {
      localStorage.removeItem(this.TOKEN_KEY);
      localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    }
    this.isAuthenticatedSubject.next(false);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    if (!this.isBrowser) {
      return null;
    }
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    if (!this.isBrowser) {
      return null;
    }
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    if (!this.hasToken()) {
      return false;
    }
    
    // Check if token is expired (basic check)
    const token = this.getToken();
    if (!token) {
      return false;
    }
    
    // Decode JWT token to check expiration
    try {
      const payload = this.decodeToken(token);
      if (!payload || !payload.exp) {
        return true; // If no exp claim, assume valid (server will validate)
      }
      
      const expirationDate = new Date(payload.exp * 1000);
      const isExpired = expirationDate <= new Date();
      
      if (isExpired) {
        // Token expired, clear it
        this.logout();
        return false;
      }
      
      return true;
    } catch (error) {
      // If token decode fails, clear it
      console.error('Token validation error:', error);
      this.clearTokens();
      return false;
    }
  }

  private decodeToken(token: string): any {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) {
        return null;
      }
      
      const payload = parts[1];
      const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(decoded);
    } catch (error) {
      return null;
    }
  }

  private clearTokens(): void {
    if (this.isBrowser) {
      localStorage.removeItem(this.TOKEN_KEY);
      localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    }
    this.isAuthenticatedSubject.next(false);
  }

  private storeToken(token: string): void {
    if (this.isBrowser) {
      localStorage.setItem(this.TOKEN_KEY, token);
    }
  }

  private storeRefreshToken(token: string): void {
    if (this.isBrowser) {
      localStorage.setItem(this.REFRESH_TOKEN_KEY, token);
    }
  }

  private hasToken(): boolean {
    if (!this.isBrowser) {
      return false;
    }
    return !!localStorage.getItem(this.TOKEN_KEY);
  }
}
