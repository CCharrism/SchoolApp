import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();
  
  console.log('🔗 Interceptor - Request URL:', req.url);
  console.log('🔑 Interceptor - Token available:', !!token);
  console.log('✅ Interceptor - Is authenticated:', authService.isAuthenticated());
  
  if (token && authService.isAuthenticated()) {
    console.log('🚀 Interceptor - Adding token to request');
    const authReq = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`)
    });
    return next(authReq);
  } else {
    console.log('❌ Interceptor - No token or not authenticated, sending request without auth');
  }
  
  return next(req);
};
