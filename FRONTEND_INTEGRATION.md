# Bus Ticket Booking API - Frontend Integration Guide

A complete guide for frontend developers to integrate with the Bus Ticket Booking API.

---

## ?? Table of Contents

1. [API Base URL](#api-base-url)
2. [Authentication Flow](#authentication-flow)
3. [HTTP Headers](#http-headers)
4. [Response Format](#response-format)
5. [Error Handling](#error-handling)
6. [Role-Based Access](#role-based-access)
7. [API Integration Examples](#api-integration-examples)
8. [TypeScript Interfaces](#typescript-interfaces)
9. [React Integration Examples](#react-integration-examples)
10. [Operator Dashboard Integration](#operator-dashboard-integration)
11. [Admin Panel Integration](#admin-panel-integration)
12. [Complete User Flows](#complete-user-flows)

---

## ?? API Base URL

```
Development: http://localhost:5129/api
Production:  https://your-domain.com/api
```

---

## ?? Authentication Flow

### Overview

The API uses a **token-based authentication** system with in-memory distributed caching. After login, include the token in all authenticated requests using the `X-User-Token` header.

```
???????????????????????????????????????????????????????????????????
?                    AUTHENTICATION FLOW                          ?
???????????????????????????????????????????????????????????????????
?                                                                 ?
?  1. LOGIN                                                       ?
?     POST /api/auth/login                                        ?
?     Body: { "email": "...", "password": "..." }                 ?
?     ??? Returns token (valid for 24 hours)                     ?
?                                                                 ?
?  2. STORE TOKEN                                                 ?
?     localStorage.setItem('authToken', response.data.token)      ?
?     localStorage.setItem('user', JSON.stringify(response.data)) ?
?                                                                 ?
?  3. USE TOKEN IN REQUESTS                                       ?
?     Header: X-User-Token: YOUR_TOKEN_HERE                       ?
?     ??? All authenticated endpoints require this header         ?
?                                                                 ?
?  4. HANDLE 401 ERRORS                                           ?
?     If 401 received ? Token expired ? Redirect to login         ?
?                                                                 ?
?  5. LOGOUT                                                      ?
?     POST /api/auth/logout with X-User-Token header              ?
?     ??? Token is invalidated server-side                       ?
?                                                                 ?
???????????????????????????????????????????????????????????????????
```

### Token Details

| Property | Value |
|----------|-------|
| Header Name | `X-User-Token` |
| Token Format | Base64 URL-safe string |
| Expiration | 24 hours from login |
| Storage | Server-side distributed cache |

### Token Storage Recommendation

```javascript
// Store token after login
const handleLoginSuccess = (response) => {
  localStorage.setItem('authToken', response.data.token);
  localStorage.setItem('user', JSON.stringify(response.data));
  localStorage.setItem('userRole', response.data.role);
};

// Retrieve token for requests
const getToken = () => localStorage.getItem('authToken');

// Get user role for conditional rendering
const getUserRole = () => localStorage.getItem('userRole');

// Clear on logout
const handleLogout = () => {
  localStorage.removeItem('authToken');
  localStorage.removeItem('user');
  localStorage.removeItem('userRole');
};
```

### Test Credentials

| Role | Email | Password | Access |
|------|-------|----------|--------|
| Admin | admin@busbooking.com | Password@123 | Full system access |
| Operator | operator1@redbus.com | Password@123 | Bus/Schedule/Trip management |
| Passenger | john.doe@gmail.com | Password@123 | Booking/Profile access |

---

## ?? HTTP Headers

### Required Headers for All Requests

```javascript
const headers = {
  'Content-Type': 'application/json',
  'Accept': 'application/json'
};
```

### Additional Header for Authenticated Requests

```javascript
const authHeaders = {
  'Content-Type': 'application/json',
  'Accept': 'application/json',
  'X-User-Token': localStorage.getItem('authToken')
};
```

---

## ?? Response Format

### Success Response

```typescript
interface ApiResponse<T> {
  success: true;
  message: string;
  data: T;
  errors: null;
}
```

### Error Response

```typescript
interface ApiErrorResponse {
  success: false;
  message: string;
  data: null;
  errors: string[] | null;
}
```

### Example Success Response

```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "userId": "33333333-3333-3333-3333-333333333331",
    "email": "john.doe@gmail.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Passenger",
    "token": "abc123xyz...",
    "expiresAt": "2025-02-01T12:00:00Z"
  },
  "errors": null
}
```

### Example Error Response

```json
{
  "success": false,
  "message": "Not authorized as operator",
  "data": null,
  "errors": null
}
```

---

## ?? Error Handling

### HTTP Status Codes

| Code | Meaning | Action |
|------|---------|--------|
| 200 | Success | Process response data |
| 201 | Created | Resource created successfully |
| 400 | Bad Request | Show validation errors to user |
| 401 | Unauthorized | Token missing/expired - redirect to login |
| 403 | Forbidden | User doesn't have required role |
| 404 | Not Found | Show "not found" message |
| 409 | Conflict | Show conflict message (e.g., email exists) |
| 500 | Server Error | Show generic error message |

### Error Handler Example

```javascript
const handleApiError = (error) => {
  if (error.response) {
    const { status, data } = error.response;
    
    switch (status) {
      case 400:
        return data.errors?.join(', ') || data.message;
      case 401:
        // Token expired or invalid
        localStorage.removeItem('authToken');
        localStorage.removeItem('user');
        localStorage.removeItem('userRole');
        window.location.href = '/login';
        return 'Session expired. Please login again.';
      case 403:
        return 'You do not have permission to perform this action.';
      case 404:
        return data.message || 'Resource not found.';
      case 409:
        return data.message || 'Conflict occurred.';
      default:
        return 'An unexpected error occurred. Please try again.';
    }
  }
  return 'Network error. Please check your connection.';
};
```

---

## ?? Role-Based Access

### Endpoint Access by Role

| Endpoint Category | Public | Passenger | Operator | Admin |
|-------------------|--------|-----------|----------|-------|
| Search/Routes/Trips | ? | ? | ? | ? |
| Auth (Login/Register) | ? | ? | ? | ? |
| User Profile | ? | ? | ? | ? |
| Bookings (Own) | ? | ? | ? | ? |
| Reviews (Create) | ? | ? | ? | ? |
| Notifications | ? | ? | ? | ? |
| `/api/operator/*` | ? | ? | ? | ? |
| `/api/admin/*` | ? | ? | ? | ? |

### Role-Based Routing (React Example)

```tsx
// components/ProtectedRoute.tsx
import { Navigate } from 'react-router-dom';

interface Props {
  children: React.ReactNode;
  requiredRole?: 'Passenger' | 'Operator' | 'Admin';
}

const ProtectedRoute: React.FC<Props> = ({ children, requiredRole }) => {
  const token = localStorage.getItem('authToken');
  const userRole = localStorage.getItem('userRole');

  if (!token) {
    return <Navigate to="/login" replace />;
  }

  if (requiredRole && userRole !== requiredRole) {
    return <Navigate to="/unauthorized" replace />;
  }

  return <>{children}</>;
};

// Usage in App.tsx
<Route 
  path="/operator/dashboard" 
  element={
    <ProtectedRoute requiredRole="Operator">
      <OperatorDashboard />
    </ProtectedRoute>
  } 
/>

<Route 
  path="/admin/dashboard" 
  element={
    <ProtectedRoute requiredRole="Admin">
      <AdminDashboard />
    </ProtectedRoute>
  } 
/>

---

## ?? API Integration Examples

### Axios Setup

```javascript
// api/axiosConfig.js
import axios from 'axios';

const API_BASE_URL = 'http://localhost:5129/api';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor - add auth token
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers['X-User-Token'] = token;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor - handle errors
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('authToken');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default apiClient;
```

### Fetch API Setup

```javascript
// api/fetchConfig.js
const API_BASE_URL = 'http://localhost:5129/api';

const getHeaders = () => {
  const headers = {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
  };
  
  const token = localStorage.getItem('authToken');
  if (token) {
    headers['X-User-Token'] = token;
  }
  
  return headers;
};

export const apiRequest = async (endpoint, options = {}) => {
  const url = `${API_BASE_URL}${endpoint}`;
  
  const config = {
    ...options,
    headers: {
      ...getHeaders(),
      ...options.headers,
    },
  };

  const response = await fetch(url, config);
  const data = await response.json();

  if (!response.ok) {
    if (response.status === 401) {
      localStorage.removeItem('authToken');
      window.location.href = '/login';
    }
    throw { response: { status: response.status, data } };
  }

  return data;
};

export const get = (endpoint) => apiRequest(endpoint, { method: 'GET' });
export const post = (endpoint, body) => apiRequest(endpoint, { method: 'POST', body: JSON.stringify(body) });
export const put = (endpoint, body) => apiRequest(endpoint, { method: 'PUT', body: JSON.stringify(body) });
export const patch = (endpoint, body) => apiRequest(endpoint, { method: 'PATCH', body: JSON.stringify(body) });
export const del = (endpoint) => apiRequest(endpoint, { method: 'DELETE' });
```

---

## ?? TypeScript Interfaces

### Authentication Types

```typescript
// types/auth.ts

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: 'Passenger' | 'Operator' | 'Admin';
  token: string;
  expiresAt: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  dateOfBirth: string;
  gender: string;
}

export interface RegisterResponse {
  userId: string;
  email: string;
  message: string;
}
```

### User Types

```typescript
// types/user.ts

export interface UserProfile {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  dateOfBirth: string;
  gender: string;
  role: string;
  isVerified: boolean;
  createdAt: string;
}

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  phoneNumber: string;
  dateOfBirth: string;
  gender: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}
```

### Search Types

```typescript
// types/search.ts

export interface SearchQuery {
  source: string;
  destination: string;
  date: string;
  passengers?: number;
  busType?: 'AC' | 'NonAC';
  busCategory?: 'Sleeper' | 'Seater' | 'SemiSleeper';
  minPrice?: number;
  maxPrice?: number;
  departureTimeSlot?: 'Morning' | 'Afternoon' | 'Evening' | 'Night';
  sortBy?: 'price' | 'duration' | 'departure' | 'rating';
  sortOrder?: 'asc' | 'desc';
}

export interface SearchResult {
  tripId: string;
  busId: string;
  busNumber: string;
  operatorName: string;
  operatorRating: number;
  busType: string;
  busCategory: string;
  departureTime: string;
  arrivalTime: string;
  duration: string;
  source: string;
  destination: string;
  fare: number;
  availableSeats: number;
  amenities: string[];
}

export interface Route {
  routeId: string;
  sourceCity: string;
  destinationCity: string;
  distanceKm: number;
  estimatedDurationHours: number;
}
```

### Trip Types

```typescript
// types/trip.ts

export interface TripDetails {
  tripId: string;
  busNumber: string;
  operatorName: string;
  busType: string;
  busCategory: string;
  source: string;
  destination: string;
  departureDateTime: string;
  arrivalDateTime: string;
  duration: string;
  fare: number;
  availableSeats: number;
  totalSeats: number;
  amenities: string[];
  boardingPoints: Stop[];
  droppingPoints: Stop[];
}

export interface Stop {
  stopId: string;
  stopName: string;
  arrivalTime: string;
  departureTime: string;
}

export interface Seat {
  seatNumber: string;
  seatType: 'Lower' | 'Upper' | 'Window' | 'Aisle';
  deck: 'Lower' | 'Upper';
  positionX: number;
  positionY: number;
  isAvailable: boolean;
  price: number;
}

export interface SeatLayout {
  busId: string;
  totalSeats: number;
  availableSeats: number;
  lowerDeck: Seat[];
  upperDeck: Seat[];
}
```

### Booking Types

```typescript
// types/booking.ts

export interface PassengerInfo {
  name: string;
  age: number;
  gender: string;
  seatNumber: string;
}

export interface CreateBookingRequest {
  tripId: string;
  passengers: PassengerInfo[];
  boardingPointId: string;
  droppingPointId: string;
  contactEmail: string;
  contactPhone: string;
  offerCode?: string;
}

export interface BookingResponse {
  bookingId: string;
  bookingReference: string;
  tripId: string;
  source: string;
  destination: string;
  travelDate: string;
  departureTime: string;
  busNumber: string;
  operatorName: string;
  passengers: PassengerInfo[];
  boardingPoint: string;
  droppingPoint: string;
  totalSeats: number;
  baseFare: number;
  discount: number;
  totalFare: number;
  bookingStatus: string;
  bookingDate: string;
}

export interface BookingHistory {
  bookingId: string;
  bookingReference: string;
  source: string;
  destination: string;
  travelDate: string;
  totalSeats: number;
  totalFare: number;
  bookingStatus: string;
  bookingDate: string;
}
```

### Payment Types

```typescript
// types/payment.ts

export type PaymentMethod = 'Card' | 'UPI' | 'Wallet' | 'NetBanking';
export type PaymentStatus = 'Pending' | 'Completed' | 'Failed' | 'Refunded';

export interface InitiatePaymentRequest {
  bookingId: string;
  paymentMethod: PaymentMethod;
}

export interface PaymentResponse {
  paymentId: string;
  bookingId: string;
  amount: number;
  paymentMethod: string;
  paymentStatus: PaymentStatus;
  transactionId: string | null;
  paymentDate: string | null;
}

export interface CompletePaymentRequest {
  transactionId: string;
}
```

### Cancellation Types

```typescript
// types/cancellation.ts

export interface CancellationPolicy {
  policyDescription: string;
  slabs: CancellationSlab[];
}

export interface CancellationSlab {
  hoursBeforeDeparture: number;
  cancellationChargePercentage: number;
  description: string;
}

export interface CalculateRefundRequest {
  bookingId: string;
}

export interface RefundCalculation {
  bookingId: string;
  totalFare: number;
  cancellationCharges: number;
  refundAmount: number;
  appliedSlab: string;
}

export interface CreateCancellationRequest {
  bookingId: string;
  reason?: string;
}

export interface CancellationResponse {
  cancellationId: string;
  bookingId: string;
  bookingReference: string;
  refundAmount: number;
  cancellationCharges: number;
  refundStatus: string;
  cancellationDate: string;
  message: string;
}
```

### Offer Types

```typescript
// types/offer.ts

export interface Offer {
  offerId: string;
  offerCode: string;
  description: string;
  discountType: 'Percentage' | 'Flat';
  discountValue: number;
  minBookingAmount: number;
  maxDiscount: number;
  validFrom: string;
  validTo: string;
}

export interface ValidateOfferRequest {
  offerCode: string;
  bookingAmount: number;
}

export interface ValidateOfferResponse {
  isValid: boolean;
  discountAmount: number;
  finalAmount: number;
  message: string;
}
```

### Review Types

```typescript
// types/review.ts

export interface Review {
  reviewId: string;
  userId: string;
  userName: string;
  rating: number;
  comment: string;
  createdAt: string;
}

export interface CreateReviewRequest {
  tripId: string;
  rating: number;
  comment: string;
}

export interface UpdateReviewRequest {
  rating: number;
  comment: string;
}
```

### Notification Types

```typescript
// types/notification.ts

export interface Notification {
  notificationId: string;
  notificationType: 'Email' | 'SMS' | 'Push';
  subject: string;
  message: string;
  isRead: boolean;
  sentAt: string;
  createdAt: string;
}
```

### Common Types

```typescript
// types/common.ts

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: string[] | null;
}

export interface PaginationQuery {
  pageNumber?: number;
  pageSize?: number;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
```

---

## ?? React Integration Examples

### API Service Layer

```typescript
// services/authService.ts
import apiClient from '../api/axiosConfig';
import { LoginRequest, LoginResponse, RegisterRequest, RegisterResponse } from '../types/auth';
import { ApiResponse } from '../types/common';

export const authService = {
  login: async (credentials: LoginRequest): Promise<ApiResponse<LoginResponse>> => {
    const response = await apiClient.post('/auth/login', credentials);
    return response.data;
  },

  register: async (userData: RegisterRequest): Promise<ApiResponse<RegisterResponse>> => {
    const response = await apiClient.post('/auth/register', userData);
    return response.data;
  },

  logout: async (): Promise<void> => {
    await apiClient.post('/auth/logout');
    localStorage.removeItem('authToken');
    localStorage.removeItem('user');
  },

  verifyEmail: async (email: string, code: string): Promise<ApiResponse<any>> => {
    const response = await apiClient.post('/auth/verify-email', { email, verificationCode: code });
    return response.data;
  },
};
```

```typescript
// services/searchService.ts
import apiClient from '../api/axiosConfig';
import { SearchQuery, SearchResult, Route } from '../types/search';
import { ApiResponse } from '../types/common';

export const searchService = {
  searchBuses: async (query: SearchQuery): Promise<ApiResponse<SearchResult[]>> => {
    const params = new URLSearchParams();
    Object.entries(query).forEach(([key, value]) => {
      if (value !== undefined) params.append(key, String(value));
    });
    const response = await apiClient.get(`/search?${params.toString()}`);
    return response.data;
  },

  getRoutes: async (): Promise<ApiResponse<Route[]>> => {
    const response = await apiClient.get('/search/routes');
    return response.data;
  },

  getRouteDetails: async (routeId: string): Promise<ApiResponse<Route>> => {
    const response = await apiClient.get(`/search/routes/${routeId}`);
    return response.data;
  },
};
```

```typescript
// services/bookingService.ts
import apiClient from '../api/axiosConfig';
import { CreateBookingRequest, BookingResponse, BookingHistory } from '../types/booking';
import { ApiResponse } from '../types/common';

export const bookingService = {
  createBooking: async (booking: CreateBookingRequest): Promise<ApiResponse<BookingResponse>> => {
    const response = await apiClient.post('/bookings', booking);
    return response.data;
  },

  getBookingDetails: async (bookingId: string): Promise<ApiResponse<BookingResponse>> => {
    const response = await apiClient.get(`/bookings/${bookingId}`);
    return response.data;
  },

  getBookingByReference: async (reference: string): Promise<ApiResponse<BookingResponse>> => {
    const response = await apiClient.get(`/bookings/reference/${reference}`);
    return response.data;
  },

  getUserBookings: async (page = 1, pageSize = 10): Promise<ApiResponse<BookingHistory[]>> => {
    const response = await apiClient.get(`/users/profile/bookings?pageNumber=${page}&pageSize=${pageSize}`);
    return response.data;
  },

  confirmBooking: async (bookingId: string): Promise<ApiResponse<any>> => {
    const response = await apiClient.post(`/bookings/${bookingId}/confirm`);
    return response.data;
  },
};
```

### React Hooks

```typescript
// hooks/useAuth.ts
import { useState, useEffect, createContext, useContext } from 'react';
import { authService } from '../services/authService';
import { LoginRequest, LoginResponse } from '../types/auth';

interface AuthContextType {
  user: LoginResponse | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: LoginRequest) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<LoginResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const storedUser = localStorage.getItem('user');
    if (storedUser) {
      setUser(JSON.parse(storedUser));
    }
    setIsLoading(false);
  }, []);

  const login = async (credentials: LoginRequest) => {
    const response = await authService.login(credentials);
    if (response.success && response.data) {
      localStorage.setItem('authToken', response.data.token);
      localStorage.setItem('user', JSON.stringify(response.data));
      setUser(response.data);
    } else {
      throw new Error(response.message);
    }
  };

  const logout = async () => {
    await authService.logout();
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, isAuthenticated: !!user, isLoading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within AuthProvider');
  return context;
};
```

```typescript
// hooks/useSearch.ts
import { useState } from 'react';
import { searchService } from '../services/searchService';
import { SearchQuery, SearchResult } from '../types/search';

export const useSearch = () => {
  const [results, setResults] = useState<SearchResult[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const search = async (query: SearchQuery) => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await searchService.searchBuses(query);
      if (response.success) {
        setResults(response.data || []);
      } else {
        setError(response.message);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Search failed');
    } finally {
      setIsLoading(false);
    }
  };

  return { results, isLoading, error, search };
};
```

### React Components

```tsx
// components/LoginForm.tsx
import React, { useState } from 'react';
import { useAuth } from '../hooks/useAuth';

const LoginForm: React.FC = () => {
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError('');

    try {
      await login({ email, password });
      // Redirect to dashboard or home
      window.location.href = '/dashboard';
    } catch (err: any) {
      setError(err.message || 'Login failed');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      {error && <div className="error-message">{error}</div>}
      
      <div className="form-group">
        <label htmlFor="email">Email</label>
        <input
          type="email"
          id="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />
      </div>

      <div className="form-group">
        <label htmlFor="password">Password</label>
        <input
          type="password"
          id="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
      </div>

      <button type="submit" disabled={isLoading}>
        {isLoading ? 'Logging in...' : 'Login'}
      </button>
    </form>
  );
};

export default LoginForm;
```

```tsx
// components/BusSearch.tsx
import React, { useState } from 'react';
import { useSearch } from '../hooks/useSearch';
import { SearchQuery } from '../types/search';

const BusSearch: React.FC = () => {
  const { results, isLoading, error, search } = useSearch();
  const [formData, setFormData] = useState<SearchQuery>({
    source: '',
    destination: '',
    date: '',
    passengers: 1,
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    search(formData);
  };

  return (
    <div>
      <form onSubmit={handleSubmit}>
        <input
          type="text"
          placeholder="From"
          value={formData.source}
          onChange={(e) => setFormData({ ...formData, source: e.target.value })}
          required
        />
        <input
          type="text"
          placeholder="To"
          value={formData.destination}
          onChange={(e) => setFormData({ ...formData, destination: e.target.value })}
          required
        />
        <input
          type="date"
          value={formData.date}
          onChange={(e) => setFormData({ ...formData, date: e.target.value })}
          required
        />
        <input
          type="number"
          min="1"
          max="6"
          value={formData.passengers}
          onChange={(e) => setFormData({ ...formData, passengers: parseInt(e.target.value) })}
        />
        <button type="submit" disabled={isLoading}>
          {isLoading ? 'Searching...' : 'Search Buses'}
        </button>
      </form>

      {error && <div className="error">{error}</div>}

      <div className="results">
        {results.map((bus) => (
          <div key={bus.tripId} className="bus-card">
            <h3>{bus.operatorName}</h3>
            <p>{bus.busNumber} - {bus.busType} {bus.busCategory}</p>
            <p>{bus.departureTime} ? {bus.arrivalTime}</p>
            <p>?{bus.fare} | {bus.availableSeats} seats available</p>
            <button onClick={() => window.location.href = `/book/${bus.tripId}`}>
              Select Seats
            </button>
          </div>
        ))}
      </div>
    </div>
  );
};

export default BusSearch;
```

```tsx
// components/SeatSelection.tsx
import React, { useState, useEffect } from 'react';
import apiClient from '../api/axiosConfig';
import { Seat, SeatLayout } from '../types/trip';

interface Props {
  tripId: string;
  onSeatsSelected: (seats: string[]) => void;
}

const SeatSelection: React.FC<Props> = ({ tripId, onSeatsSelected }) => {
  const [seatLayout, setSeatLayout] = useState<SeatLayout | null>(null);
  const [selectedSeats, setSelectedSeats] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchSeats = async () => {
      try {
        const response = await apiClient.get(`/trips/${tripId}/seats`);
        if (response.data.success) {
          setSeatLayout(response.data.data);
        }
      } catch (error) {
        console.error('Failed to fetch seats:', error);
      } finally {
        setIsLoading(false);
      }
    };
    fetchSeats();
  }, [tripId]);

  const toggleSeat = (seatNumber: string, isAvailable: boolean) => {
    if (!isAvailable) return;

    setSelectedSeats((prev) => {
      const newSelection = prev.includes(seatNumber)
        ? prev.filter((s) => s !== seatNumber)
        : [...prev, seatNumber];
      
      onSeatsSelected(newSelection);
      return newSelection;
    });
  };

  const renderSeat = (seat: Seat) => {
    const isSelected = selectedSeats.includes(seat.seatNumber);
    const className = `seat ${!seat.isAvailable ? 'booked' : isSelected ? 'selected' : 'available'}`;

    return (
      <button
        key={seat.seatNumber}
        className={className}
        onClick={() => toggleSeat(seat.seatNumber, seat.isAvailable)}
        disabled={!seat.isAvailable}
        style={{
          gridColumn: seat.positionX + 1,
          gridRow: seat.positionY + 1,
        }}
      >
        {seat.seatNumber}
      </button>
    );
  };

  if (isLoading) return <div>Loading seats...</div>;
  if (!seatLayout) return <div>Failed to load seat layout</div>;

  return (
    <div className="seat-selection">
      <div className="deck">
        <h4>Lower Deck</h4>
        <div className="seat-grid">
          {seatLayout.lowerDeck.map(renderSeat)}
        </div>
      </div>

      {seatLayout.upperDeck.length > 0 && (
        <div className="deck">
          <h4>Upper Deck</h4>
          <div className="seat-grid">
            {seatLayout.upperDeck.map(renderSeat)}
          </div>
        </div>
      )}

      <div className="legend">
        <span className="available">Available</span>
        <span className="selected">Selected</span>
        <span className="booked">Booked</span>
      </div>

      <p>Selected: {selectedSeats.join(', ') || 'None'}</p>
    </div>
  );
};

export default SeatSelection;
```

---

## ?? Complete User Flows

### 1. Search & Book Flow

```
????????????????????????????????????????????????????????????????????
?                        SEARCH & BOOK FLOW                        ?
????????????????????????????????????????????????????????????????????
?                                                                  ?
?  1. SEARCH                                                       ?
?     GET /search?source=Bangalore&destination=Chennai&date=...   ?
?     ??? Display list of available buses                         ?
?                                                                  ?
?  2. SELECT BUS                                                   ?
?     GET /trips/{tripId}                                          ?
?     GET /trips/{tripId}/seats                                    ?
?     GET /trips/{tripId}/stops                                    ?
?     ??? Show bus details, seat layout, boarding/dropping points ?
?                                                                  ?
?  3. SELECT SEATS & ENTER PASSENGER DETAILS                       ?
?     ??? User selects seats and enters passenger info            ?
?                                                                  ?
?  4. APPLY OFFER (Optional)                                       ?
?     POST /offers/validate                                        ?
?     ??? Validate and show discount                              ?
?                                                                  ?
?  5. CREATE BOOKING                                               ?
?     POST /bookings (with X-User-Token)                           ?
?     ??? Returns bookingId and booking details                   ?
?                                                                  ?
?  6. INITIATE PAYMENT                                             ?
?     POST /payments                                               ?
?     ??? Returns paymentId                                       ?
?                                                                  ?
?  7. COMPLETE PAYMENT                                             ?
?     POST /payments/{paymentId}/complete                          ?
?     ??? Payment confirmed                                       ?
?                                                                  ?
?  8. CONFIRM BOOKING                                              ?
?     POST /bookings/{bookingId}/confirm                           ?
?     ??? Booking confirmed, show ticket                          ?
?                                                                  ?
????????????????????????????????????????????????????????????????????
```

### 2. Cancellation Flow

```
????????????????????????????????????????????????????????????????????
?                      CANCELLATION FLOW                           ?
????????????????????????????????????????????????????????????????????
?                                                                  ?
?  1. VIEW BOOKING                                                 ?
?     GET /bookings/{bookingId}                                    ?
?     ??? Show booking details with cancel option                 ?
?                                                                  ?
?  2. CHECK CANCELLATION POLICY                                    ?
?     GET /cancellations/policy                                    ?
?     ??? Display cancellation charges                            ?
?                                                                  ?
?  3. CALCULATE REFUND                                             ?
?     POST /cancellations/calculate                                ?
?     ??? Show refund amount before confirming                    ?
?                                                                  ?
?  4. CONFIRM CANCELLATION                                         ?
?     POST /cancellations                                          ?
?     ??? Booking cancelled, refund initiated                     ?
?                                                                  ?
?  5. CHECK REFUND STATUS                                          ?
?     GET /refunds/cancellation/{cancellationId}                   ?
?     ??? Show refund status                                      ?
?                                                                  ?
????????????????????????????????????????????????????????????????????
```

### 3. User Registration & Profile Flow

```
????????????????????????????????????????????????????????????????????
?                   REGISTRATION & PROFILE FLOW                    ?
????????????????????????????????????????????????????????????????????
?                                                                  ?
?  1. REGISTER                                                     ?
?     POST /auth/register                                          ?
?     ??? Account created, verification email sent                ?
?                                                                  ?
?  2. VERIFY EMAIL                                                 ?
?     POST /auth/verify-email                                      ?
?     ??? Email verified                                          ?
?                                                                  ?
?  3. LOGIN                                                        ?
?     POST /auth/login                                             ?
?     ??? Store token, redirect to dashboard                      ?
?                                                                  ?
?  4. VIEW PROFILE                                                 ?
?     GET /users/profile                                           ?
?     ??? Display user profile                                    ?
?                                                                  ?
?  5. UPDATE PROFILE                                               ?
?     PUT /users/profile                                           ?
?     ??? Profile updated                                         ?
?                                                                  ?
?  6. VIEW BOOKING HISTORY                                         ?
?     GET /users/profile/bookings                                  ?
?     ??? Display past bookings                                   ?
?                                                                  ?
?  7. VIEW NOTIFICATIONS                                           ?
?     GET /notifications                                           ?
?     ??? Display notifications                                   ?
?                                                                  ?
?  8. LOGOUT                                                       ?
?     POST /auth/logout                                            ?
?     ??? Clear token, redirect to home                           ?
?                                                                  ?
????????????????????????????????????????????????????????????????????
```

### 4. Operator Schedule Management Flow

```
????????????????????????????????????????????????????????????????????
?                 OPERATOR SCHEDULE MANAGEMENT FLOW                ?
????????????????????????????????????????????????????????????????????
?                                                                  ?
?  1. LOGIN AS OPERATOR                                            ?
?     POST /auth/login                                             ?
?     Email: operator1@redbus.com                                  ?
?     ??? Get operator token                                      ?
?                                                                  ?
?  2. VIEW DASHBOARD                                               ?
?     GET /operator/dashboard                                      ?
?     Header: X-User-Token: OPERATOR_TOKEN                         ?
?     ??? See overall statistics                                  ?
?                                                                  ?
?  3. CHECK EXISTING BUSES                                         ?
?     GET /operator/buses                                          ?
?     ??? List of operator's buses                                ?
?                                                                  ?
?  4. CREATE NEW BUS (if needed)                                   ?
?     POST /operator/buses                                         ?
?     Body: { busNumber, busType, busCategory, totalSeats, ... }   ?
?     ??? New bus created                                         ?
?                                                                  ?
?  5. CHECK EXISTING ROUTES                                        ?
?     GET /operator/routes                                         ?
?     ??? List of available routes                                ?
?                                                                  ?
?  6. CREATE NEW ROUTE (if needed)                                 ?
?     POST /operator/routes                                        ?
?     Body: { sourceCity, destinationCity, distanceKm, ... }       ?
?     ??? New route created                                       ?
?                                                                  ?
?  7. CREATE SCHEDULE                                              ?
?     POST /operator/schedules                                     ?
?     Body: { busId, routeId, departureTime, arrivalTime, fare }   ?
?     ??? Schedule created                                        ?
?                                                                  ?
?  8. VIEW SCHEDULES                                               ?
?     GET /operator/schedules                                      ?
?     ??? All operator's schedules                                ?
?                                                                  ?
?  9. VIEW TRIPS & BOOKINGS                                        ?
?     GET /operator/trips                                          ?
?     GET /operator/trips/{tripId}/bookings                        ?
?     ??? Trip details and passenger list                         ?
?                                                                  ?
?  10. VIEW REVENUE                                                ?
?      GET /operator/revenue?startDate=...&endDate=...             ?
?      ??? Revenue statistics and breakdown                       ?
?                                                                  ?
????????????????????????????????????????????????????????????????????
```

### 5. Admin Management Flow

```
????????????????????????????????????????????????????????????????????
?                     ADMIN MANAGEMENT FLOW                        ?
????????????????????????????????????????????????????????????????????
?                                                                  ?
?  1. LOGIN AS ADMIN                                               ?
?     POST /auth/login                                             ?
?     Email: admin@busbooking.com                                  ?
?     ??? Get admin token                                         ?
?                                                                  ?
?  2. VIEW DASHBOARD                                               ?
?     GET /admin/dashboard                                         ?
?     Header: X-User-Token: ADMIN_TOKEN                            ?
?     ??? System-wide statistics                                  ?
?                                                                  ?
?  3. REVIEW PENDING OPERATORS                                     ?
?     GET /admin/operators/pending                                 ?
?     ??? List of operators awaiting approval                     ?
?                                                                  ?
?  4. APPROVE/REJECT OPERATOR                                      ?
?     POST /admin/operators/approve                                ?
?     Body: { operatorId: "..." }                                  ?
?     ??? Operator approved/rejected                              ?
?                                                                  ?
?  5. MANAGE USERS                                                 ?
?     GET /admin/users                                             ?
?     PATCH /admin/users/{userId}/status                           ?
?     ??? View and manage user accounts                           ?
?                                                                  ?
?  6. VIEW ALL BOOKINGS                                            ?
?     GET /admin/bookings                                          ?
?     GET /admin/bookings/statistics                               ?
?     ??? System-wide booking information                         ?
?                                                                  ?
?  7. MANAGE OFFERS                                                ?
?     GET /admin/offers                                            ?
?     POST /admin/offers                                           ?
?     PUT /admin/offers/{offerId}                                  ?
?     ??? Create and manage discount offers                       ?
?                                                                  ?
?  8. VIEW ANALYTICS                                               ?
?     GET /admin/analytics?metric=revenue                          ?
?     GET /admin/reports?reportType=bookings                       ?
?     ??? Detailed analytics and reports                          ?
?                                                                  ?
?  9. MANAGE REFUNDS                                               ?
?     GET /admin/payments/failed                                   ?
?     POST /admin/refunds/{refundId}/approve                       ?
?     ??? Handle payment issues and refunds                       ?
?                                                                  ?
????????????????????????????????????????????????????????????????????
```

---

## ?? CSS for Seat Selection

```css
/* styles/seat-selection.css */

.seat-selection {
  padding: 20px;
}

.deck {
  margin-bottom: 30px;
}

.seat-grid {
  display: grid;
  grid-template-columns: repeat(5, 50px);
  gap: 10px;
  padding: 20px;
  background: #f5f5f5;
  border-radius: 8px;
}

.seat {
  width: 45px;
  height: 45px;
  border: 2px solid #ccc;
  border-radius: 8px;
  cursor: pointer;
  font-size: 12px;
  font-weight: bold;
  transition: all 0.2s;
}

.seat.available {
  background: #fff;
  border-color: #4CAF50;
  color: #4CAF50;
}

.seat.available:hover {
  background: #E8F5E9;
}

.seat.selected {
  background: #4CAF50;
  border-color: #4CAF50;
  color: #fff;
}

.seat.booked {
  background: #ccc;
  border-color: #999;
  color: #666;
  cursor: not-allowed;
}

.legend {
  display: flex;
  gap: 20px;
  margin-top: 20px;
}

.legend span {
  display: flex;
  align-items: center;
  gap: 8px;
}

.legend span::before {
  content: '';
  width: 20px;
  height: 20px;
  border-radius: 4px;
}

.legend .available::before {
  background: #fff;
  border: 2px solid #4CAF50;
}

.legend .selected::before {
  background: #4CAF50;
}

.legend .booked::before {
  background: #ccc;
}
```

---

## ?? Operator Dashboard Integration

### Operator Service Layer

```typescript
// services/operatorService.ts
import apiClient from '../api/axiosConfig';

export const operatorService = {
  // Dashboard
  getDashboard: async () => {
    const response = await apiClient.get('/operator/dashboard');
    return response.data;
  },

  getProfile: async () => {
    const response = await apiClient.get('/operator/profile');
    return response.data;
  },

  updateProfile: async (data: any) => {
    const response = await apiClient.put('/operator/profile', data);
    return response.data;
  },

  // Buses
  getBuses: async () => {
    const response = await apiClient.get('/operator/buses');
    return response.data;
  },

  createBus: async (data: any) => {
    const response = await apiClient.post('/operator/buses', data);
    return response.data;
  },

  updateBus: async (busId: string, data: any) => {
    const response = await apiClient.put(`/operator/buses/${busId}`, data);
    return response.data;
  },

  deleteBus: async (busId: string) => {
    const response = await apiClient.delete(`/operator/buses/${busId}`);
    return response.data;
  },

  // Routes
  getRoutes: async () => {
    const response = await apiClient.get('/operator/routes');
    return response.data;
  },

  createRoute: async (data: any) => {
    const response = await apiClient.post('/operator/routes', data);
    return response.data;
  },

  // Schedules
  getSchedules: async () => {
    const response = await apiClient.get('/operator/schedules');
    return response.data;
  },

  createSchedule: async (data: any) => {
    const response = await apiClient.post('/operator/schedules', data);
    return response.data;
  },

  updateSchedule: async (scheduleId: string, data: any) => {
    const response = await apiClient.put(`/operator/schedules/${scheduleId}`, data);
    return response.data;
  },

  deleteSchedule: async (scheduleId: string) => {
    const response = await apiClient.delete(`/operator/schedules/${scheduleId}`);
    return response.data;
  },

  // Trips
  getTrips: async (date?: string) => {
    const url = date ? `/operator/trips?date=${date}` : '/operator/trips';
    const response = await apiClient.get(url);
    return response.data;
  },

  getTripDetails: async (tripId: string) => {
    const response = await apiClient.get(`/operator/trips/${tripId}`);
    return response.data;
  },

  updateTripStatus: async (tripId: string, status: number) => {
    const response = await apiClient.patch(`/operator/trips/${tripId}/status`, { currentStatus: status });
    return response.data;
  },

  cancelTrip: async (tripId: string, reason: string) => {
    const response = await apiClient.post(`/operator/trips/${tripId}/cancel`, { reason });
    return response.data;
  },

  // Bookings
  getBookings: async (page = 1, pageSize = 10) => {
    const response = await apiClient.get(`/operator/bookings?pageNumber=${page}&pageSize=${pageSize}`);
    return response.data;
  },

  // Revenue
  getRevenue: async (startDate: string, endDate: string, groupBy = 'day') => {
    const response = await apiClient.get(`/operator/revenue?startDate=${startDate}&endDate=${endDate}&groupBy=${groupBy}`);
    return response.data;
  },

  exportRevenue: async (startDate: string, endDate: string) => {
    const response = await apiClient.get(`/operator/revenue/export?startDate=${startDate}&endDate=${endDate}`, {
      responseType: 'blob'
    });
    return response.data;
  },

  // Reviews
  getReviews: async (page = 1, pageSize = 10) => {
    const response = await apiClient.get(`/operator/reviews?pageNumber=${page}&pageSize=${pageSize}`);
    return response.data;
  },

  getReviewStats: async () => {
    const response = await apiClient.get('/operator/reviews/stats');
    return response.data;
  },
};
```

### Operator TypeScript Interfaces

```typescript
// types/operator.ts

export interface OperatorDashboard {
  operatorId: string;
  companyName: string;
  totalBuses: number;
  activeBuses: number;
  totalRoutes: number;
  totalSchedules: number;
  todayTrips: number;
  totalBookingsToday: number;
  todayRevenue: number;
  monthlyRevenue: number;
  averageRating: number;
  totalReviews: number;
}

export interface OperatorBus {
  busId: string;
  busNumber: string;
  busType: string;
  busCategory: string;
  totalSeats: number;
  registrationNumber: string;
  isActive: boolean;
  amenities: string[];
}

export interface CreateBusRequest {
  busNumber: string;
  busType: number; // 0 = AC, 1 = NonAC
  busCategory: number; // 0 = Sleeper, 1 = Seater, 2 = SemiSleeper
  totalSeats: number;
  registrationNumber: string;
  amenities: string[];
}

export interface OperatorSchedule {
  scheduleId: string;
  busNumber: string;
  route: string;
  departureTime: string;
  arrivalTime: string;
  baseFare: number;
  isActive: boolean;
}

export interface CreateScheduleRequest {
  busId: string;
  routeId: string;
  departureTime: string; // ISO datetime
  arrivalTime: string; // ISO datetime
  baseFare: number;
  isActive: boolean;
  availableDates?: string[];
}

export interface OperatorTrip {
  tripId: string;
  scheduleId: string;
  busNumber: string;
  route: string;
  tripDate: string;
  departureDateTime: string;
  arrivalDateTime: string;
  currentStatus: string;
  availableSeats: number;
  bookedSeats: number;
}

export interface RevenueStatistics {
  totalRevenue: number;
  totalBookings: number;
  totalPassengers: number;
  cancelledBookings: number;
  refundedAmount: number;
  netRevenue: number;
  breakdown: RevenueBreakdown[];
}

export interface RevenueBreakdown {
  period: string;
  revenue: number;
  bookings: number;
}
```

### Operator Dashboard Component

```tsx
// components/operator/OperatorDashboard.tsx
import React, { useState, useEffect } from 'react';
import { operatorService } from '../../services/operatorService';
import { OperatorDashboard as DashboardData } from '../../types/operator';

const OperatorDashboard: React.FC = () => {
  const [dashboard, setDashboard] = useState<DashboardData | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchDashboard = async () => {
      try {
        const response = await operatorService.getDashboard();
        if (response.success) {
          setDashboard(response.data);
        } else {
          setError(response.message);
        }
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to load dashboard');
      } finally {
        setIsLoading(false);
      }
    };
    fetchDashboard();
  }, []);

  if (isLoading) return <div>Loading...</div>;
  if (error) return <div className="error">{error}</div>;
  if (!dashboard) return null;

  return (
    <div className="operator-dashboard">
      <h1>Welcome, {dashboard.companyName}</h1>
      
      <div className="stats-grid">
        <div className="stat-card">
          <h3>Total Buses</h3>
          <p>{dashboard.totalBuses}</p>
          <small>{dashboard.activeBuses} active</small>
        </div>
        
        <div className="stat-card">
          <h3>Today's Trips</h3>
          <p>{dashboard.todayTrips}</p>
        </div>
        
        <div className="stat-card">
          <h3>Today's Bookings</h3>
          <p>{dashboard.totalBookingsToday}</p>
        </div>
        
        <div className="stat-card">
          <h3>Today's Revenue</h3>
          <p>?{dashboard.todayRevenue.toLocaleString()}</p>
        </div>
        
        <div className="stat-card">
          <h3>Monthly Revenue</h3>
          <p>?{dashboard.monthlyRevenue.toLocaleString()}</p>
        </div>
        
        <div className="stat-card">
          <h3>Rating</h3>
          <p>? {dashboard.averageRating.toFixed(1)}</p>
          <small>{dashboard.totalReviews} reviews</small>
        </div>
      </div>
    </div>
  );
};

export default OperatorDashboard;
```

---

## ????? Admin Panel Integration

### Admin Service Layer

```typescript
// services/adminService.ts
import apiClient from '../api/axiosConfig';

export const adminService = {
  // Dashboard
  getDashboard: async () => {
    const response = await apiClient.get('/admin/dashboard');
    return response.data;
  },

  getAnalytics: async (metric: string, startDate: string, endDate: string) => {
    const response = await apiClient.get(`/admin/analytics?metric=${metric}&startDate=${startDate}&endDate=${endDate}`);
    return response.data;
  },

  getReports: async (reportType: string, startDate: string, endDate: string) => {
    const response = await apiClient.get(`/admin/reports?reportType=${reportType}&startDate=${startDate}&endDate=${endDate}`);
    return response.data;
  },

  // Users
  getUsers: async (page = 1, pageSize = 10, role?: string) => {
    let url = `/admin/users?pageNumber=${page}&pageSize=${pageSize}`;
    if (role) url += `&role=${role}`;
    const response = await apiClient.get(url);
    return response.data;
  },

  getUserDetails: async (userId: string) => {
    const response = await apiClient.get(`/admin/users/${userId}`);
    return response.data;
  },

  updateUserStatus: async (userId: string, isActive: boolean) => {
    const response = await apiClient.patch(`/admin/users/${userId}/status`, { isActive });
    return response.data;
  },

  deleteUser: async (userId: string) => {
    const response = await apiClient.delete(`/admin/users/${userId}`);
    return response.data;
  },

  getUserStatistics: async () => {
    const response = await apiClient.get('/admin/users/statistics');
    return response.data;
  },

  // Operators
  getOperators: async (page = 1, pageSize = 10) => {
    const response = await apiClient.get(`/admin/operators?pageNumber=${page}&pageSize=${pageSize}`);
    return response.data;
  },

  getOperatorDetails: async (operatorId: string) => {
    const response = await apiClient.get(`/admin/operators/${operatorId}`);
    return response.data;
  },

  getPendingOperators: async () => {
    const response = await apiClient.get('/admin/operators/pending');
    return response.data;
  },

  approveOperator: async (operatorId: string) => {
    const response = await apiClient.post('/admin/operators/approve', { operatorId });
    return response.data;
  },

  updateOperatorStatus: async (operatorId: string, isApproved: boolean) => {
    const response = await apiClient.patch(`/admin/operators/${operatorId}/status`, { isApproved });
    return response.data;
  },

  deleteOperator: async (operatorId: string) => {
    const response = await apiClient.delete(`/admin/operators/${operatorId}`);
    return response.data;
  },

  // Bookings
  getBookings: async (page = 1, pageSize = 10, status?: string) => {
    let url = `/admin/bookings?pageNumber=${page}&pageSize=${pageSize}`;
    if (status) url += `&status=${status}`;
    const response = await apiClient.get(url);
    return response.data;
  },

  getBookingStatistics: async () => {
    const response = await apiClient.get('/admin/bookings/statistics');
    return response.data;
  },

  modifyBooking: async (bookingId: string, bookingStatus: number) => {
    const response = await apiClient.patch(`/admin/bookings/${bookingId}`, { bookingStatus });
    return response.data;
  },

  // Payments
  getPayments: async (page = 1, pageSize = 10, status?: string) => {
    let url = `/admin/payments?pageNumber=${page}&pageSize=${pageSize}`;
    if (status) url += `&status=${status}`;
    const response = await apiClient.get(url);
    return response.data;
  },

  getFailedPayments: async () => {
    const response = await apiClient.get('/admin/payments/failed');
    return response.data;
  },

  getPaymentReconciliation: async (date: string) => {
    const response = await apiClient.get(`/admin/payments/reconcile?date=${date}`);
    return response.data;
  },

  approveRefund: async (refundId: string, notes: string) => {
    const response = await apiClient.post(`/admin/refunds/${refundId}/approve`, { notes });
    return response.data;
  },

  rejectRefund: async (refundId: string, reason: string) => {
    const response = await apiClient.post(`/admin/refunds/${refundId}/reject`, { reason });
    return response.data;
  },

  // Offers
  getOffers: async (page = 1, pageSize = 10) => {
    const response = await apiClient.get(`/admin/offers?pageNumber=${page}&pageSize=${pageSize}`);
    return response.data;
  },

  createOffer: async (data: any) => {
    const response = await apiClient.post('/admin/offers', data);
    return response.data;
  },

  updateOffer: async (offerId: string, data: any) => {
    const response = await apiClient.put(`/admin/offers/${offerId}`, data);
    return response.data;
  },

  deleteOffer: async (offerId: string) => {
    const response = await apiClient.delete(`/admin/offers/${offerId}`);
    return response.data;
  },
};
```

### Admin TypeScript Interfaces

```typescript
// types/admin.ts

export interface AdminDashboard {
  totalUsers: number;
  totalOperators: number;
  pendingOperatorApprovals: number;
  totalBuses: number;
  totalRoutes: number;
  todayTrips: number;
  totalBookingsToday: number;
  todayRevenue: number;
  monthlyRevenue: number;
  activeDisputes: number;
}

export interface AdminUser {
  userId: string;
  email: string;
  fullName: string;
  phoneNumber: string;
  role: string;
  isVerified: boolean;
  createdAt: string;
}

export interface AdminOperator {
  operatorId: string;
  companyName: string;
  contactEmail: string;
  contactPhone: string;
  city: string;
  rating: number;
  isApproved: boolean;
  totalBuses: number;
  createdAt: string;
}

export interface PendingOperator {
  operatorId: string;
  companyName: string;
  licenseNumber: string;
  contactEmail: string;
  contactPhone: string;
  city: string;
  state: string;
  createdAt: string;
}

export interface BookingStatistics {
  totalBookings: number;
  confirmedBookings: number;
  cancelledBookings: number;
  completedBookings: number;
  totalRevenue: number;
  averageBookingValue: number;
  bookingsToday: number;
  bookingsThisWeek: number;
  bookingsThisMonth: number;
}

export interface CreateOfferRequest {
  offerCode: string;
  description: string;
  discountType: number; // 0 = Percentage, 1 = Flat
  discountValue: number;
  minBookingAmount: number;
  maxDiscount: number;
  validFrom: string;
  validTo: string;
  usageLimit: number;
  isActive: boolean;
}
```

---

## ?? Mobile Responsive Considerations

```javascript
// Use responsive breakpoints
const isMobile = window.innerWidth <= 768;

// Adjust seat grid for mobile
const seatGridColumns = isMobile ? 'repeat(4, 40px)' : 'repeat(5, 50px)';
```

---

## ?? Security Best Practices

1. **Never store sensitive data in localStorage** - Use httpOnly cookies for tokens in production
2. **Validate all inputs** before sending to API
3. **Implement CSRF protection** for state-changing requests
4. **Use HTTPS** in production
5. **Sanitize user inputs** to prevent XSS attacks
6. **Implement rate limiting** on the frontend for API calls

---

## ?? Support

For API issues or questions, please contact:
- GitHub: [alanjohnck](https://github.com/alanjohnck)
- Repository: [bus-ticket-api](https://github.com/alanjohnck/bus-ticket-api)
