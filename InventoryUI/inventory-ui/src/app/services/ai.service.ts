import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, of, interval, Subscription } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

export interface AIResponse {
  response: string;
  context?: string;
  suggestions?: string[];
}

export interface HealthResponse {
  service: string;
  status?: string;
  timestamp?: string;
  version?: string;
}

export interface AIRequest {
  query: string;
  context: string;
  includeLocalData: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AIService {
  private healthCheckSubscription?: Subscription;
  private isHealthy = false;
  private errorMessage = '';

  constructor(private http: HttpClient) {}

  /**
   * Check AI backend health
   */
  checkHealth(): Observable<{ isHealthy: boolean; errorMessage: string }> {
    return this.http.get<HealthResponse>('/api/ai/health')
      .pipe(
        map(response => {
          this.isHealthy = response.status === 'Healthy' || response.status === 'ok';
          this.errorMessage = this.isHealthy ? '' : (response.status     || 'AI service is not responding correctly.');
          return { isHealthy: this.isHealthy, errorMessage: this.errorMessage };
        }),
        catchError((error: HttpErrorResponse) => {
          console.error('AI Health check failed:', error);
          this.isHealthy = false;
          this.errorMessage = 'AI service is currently unavailable. Please try again later.';
          return of({ isHealthy: false, errorMessage: this.errorMessage });
        })
      );
  }

  /**
   * Start periodic health monitoring
   */
  startHealthMonitoring(callback: (isHealthy: boolean, errorMessage: string) => void): Subscription {
    return interval(30000) // Check every 30 seconds
      .subscribe(() => {
        this.checkHealth().subscribe(result => {
          callback(result.isHealthy, result.errorMessage);
        });
      });
  }

  /**
   * Load default questions/suggestions from backend
   */
  getSuggestions(): Observable<string[]> {
    return this.http.get<string[]>('/api/ai/suggestions')
      .pipe(
        catchError((error: HttpErrorResponse) => {
          console.error('Failed to load suggestions:', error);
          // Fallback to default suggestions
          return of(['help', 'add inventory', 'create bill', 'view reports']);
        })
      );
  }

  /**
   * Send question to AI backend
   */
  askQuestion(request: AIRequest): Observable<AIResponse> {
    return this.http.post<AIResponse>('/api/ai/ask', request)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          console.error('AI request failed:', error);
          let errorMsg = 'Sorry, I encountered an error while processing your request. Please try again.';
          
          if (error.status === 0) {
            errorMsg = 'Unable to connect to AI service. Please check your internet connection.';
          } else if (error.status >= 500) {
            errorMsg = 'AI service is temporarily unavailable. Please try again in a moment.';
          } else if (error.status === 429) {
            errorMsg = 'Too many requests. Please wait a moment before asking again.';
          } else if (error.status === 404) {
            errorMsg = 'AI service endpoint not found. Please contact support.';
          }
          
          return of({ response: errorMsg });
        })
      );
  }

  /**
   * Get current health status without making a new request
   */
  getCurrentHealthStatus(): { isHealthy: boolean; errorMessage: string } {
    return { isHealthy: this.isHealthy, errorMessage: this.errorMessage };
  }

  /**
   * Stop health monitoring
   */
  stopHealthMonitoring(): void {
    if (this.healthCheckSubscription) {
      this.healthCheckSubscription.unsubscribe();
      this.healthCheckSubscription = undefined;
    }
  }
}
