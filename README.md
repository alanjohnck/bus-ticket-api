# Bus Ticket Booking API

A comprehensive RESTful API for bus ticket booking system built with .NET 8 and Entity Framework Core.

## ?? Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or SQL Server Express)
- Visual Studio 2022 or VS Code

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/alanjohnck/bus-ticket-api.git
   cd bus-ticket-api
   ```

2. **Update the connection string** in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=YOUR_SERVER;Initial Catalog=Bus_Booking;Integrated Security=True;Encrypt=False"
     }
   }
   ```

3. **Run database migrations**
   ```bash
   dotnet ef database update
   ```

4. **Seed the database** (optional - for test data)
   - Run the SQL script located at `Data/SeedData.sql` in SQL Server Management Studio

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access Swagger UI**
   - Navigate to: `http://localhost:5129/swagger`

---

## ?? Authentication

This API uses a simple **token-based authentication** system with in-memory caching.

### How Authentication Works

```
???????????????????????????????????????????????????????????????????
?                    AUTHENTICATION FLOW                          ?
???????????????????????????????????????????????????????????????????
?                                                                 ?
?  1. LOGIN                                                       ?
?     POST /api/auth/login                                        ?
?     Body: { "email": "...", "password": "..." }                 ?
?     ??? Returns token in response                               ?
?                                                                 ?
?  2. STORE TOKEN                                                 ?
?     Save the token from login response                          ?
?                                                                 ?
?  3. USE TOKEN IN REQUESTS                                       ?
?     Add header: X-User-Token: YOUR_TOKEN_HERE                   ?
?     ??? All authenticated endpoints require this header         ?
?                                                                 ?
?  4. LOGOUT                                                      ?
?     POST /api/auth/logout                                       ?
?     Header: X-User-Token: YOUR_TOKEN_HERE                       ?
?     ??? Token is invalidated                                    ?
?                                                                 ?
???????????????????????????????????????????????????????????????????
```

### Token Details
- **Token Type**: Random Base64 string (URL-safe)
- **Storage**: In-memory distributed cache
- **Expiration**: 24 hours from login
- **Header Name**: `X-User-Token`

### Test Credentials (from seed data)

| Role | Email | Password | Access Level |
|------|-------|----------|--------------|
| Admin | admin@busbooking.com | Password@123 | Full system access |
| Operator | operator1@redbus.com | Password@123 | Bus/Schedule management |
| Passenger | john.doe@gmail.com | Password@123 | Booking/Profile access |

### Quick Start - Get Your Token

```bash
# 1. Login to get token
curl -X POST http://localhost:5129/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "john.doe@gmail.com", "password": "Password@123"}'

# Response contains token:
# { "data": { "token": "abc123xyz...", ... } }

# 2. Use token in authenticated requests
curl -X GET http://localhost:5129/api/users/profile \
  -H "X-User-Token: abc123xyz..."
```

---

## ?? API Endpoints & Testing Guide

### Endpoints by Authentication Level

| Level | Endpoints | Required Header |
|-------|-----------|-----------------|
| **Public** | Search, Routes, Trips, Buses, Offers | None |
| **Passenger** | Profile, Bookings, Reviews, Notifications | `X-User-Token` (any user) |
| **Operator** | `/api/operator/*` | `X-User-Token` (operator user) |
| **Admin** | `/api/admin/*` | `X-User-Token` (admin user) |

---

## ?? API Endpoints & Testing Guide

### 1. Authentication APIs

#### Register a New User
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "newuser@example.com",
  "password": "SecurePass@123",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+919876543210",
  "dateOfBirth": "1990-01-15",
  "gender": "Male"
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john.doe@gmail.com",
  "password": "Password@123"
}
```

**Response:**
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
    "token": "YOUR_TOKEN_HERE",
    "expiresAt": "2025-01-31T12:00:00Z"
  }
}
```

> ?? **Important:** Copy the `token` value and use it in the `X-User-Token` header for authenticated endpoints.

#### Logout
```http
POST /api/auth/logout
X-User-Token: YOUR_TOKEN_HERE
```

#### Verify Email
```http
POST /api/auth/verify-email
Content-Type: application/json

{
  "email": "john.doe@gmail.com",
  "verificationCode": "123456"
}
```

---

### 2. User Profile APIs

#### Get Current User Profile (Requires Authentication)
```http
GET /api/users/profile
X-User-Token: YOUR_TOKEN_HERE
```

#### Get User by ID (Public)
```http
GET /api/users/{userId}

Example: GET /api/users/33333333-3333-3333-3333-333333333331
```

#### Update Profile
```http
PUT /api/users/profile
X-User-Token: YOUR_TOKEN_HERE
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Updated",
  "phoneNumber": "+919876543210",
  "dateOfBirth": "1990-01-15",
  "gender": "Male"
}
```

#### Change Password
```http
PATCH /api/users/profile/password
X-User-Token: YOUR_TOKEN_HERE
Content-Type: application/json

{
  "currentPassword": "Password@123",
  "newPassword": "NewPassword@456"
}
```

#### Get User's Booking History
```http
GET /api/users/profile/bookings?pageNumber=1&pageSize=10
X-User-Token: YOUR_TOKEN_HERE
```

#### Get User's Reviews
```http
GET /api/users/profile/reviews?pageNumber=1&pageSize=10
X-User-Token: YOUR_TOKEN_HERE
```

---

### 3. Search APIs

#### Search Buses
```http
GET /api/search?source=Bangalore&destination=Chennai&date=2025-02-05&passengers=2
```

#### Search with Filters
```http
GET /api/search?source=Bangalore&destination=Chennai&date=2025-02-05&passengers=2&busType=AC&busCategory=Sleeper&minPrice=500&maxPrice=1500&departureTimeSlot=Night&sortBy=price&sortOrder=asc
```

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| source | string | Source city (required) |
| destination | string | Destination city (required) |
| date | date | Travel date YYYY-MM-DD (required) |
| passengers | int | Number of passengers (default: 1) |
| busType | string | AC / NonAC |
| busCategory | string | Sleeper / Seater / SemiSleeper |
| minPrice | decimal | Minimum fare |
| maxPrice | decimal | Maximum fare |
| departureTimeSlot | string | Morning / Afternoon / Evening / Night |
| sortBy | string | price / duration / departure / rating |
| sortOrder | string | asc / desc |

#### Get Available Routes
```http
GET /api/search/routes
```

#### Get Route Details
```http
GET /api/search/routes/{routeId}

Example: GET /api/search/routes/55555555-5555-5555-5555-555555555551
```

---

### 4. Trip APIs

#### Get Trip Details
```http
GET /api/trips/{tripId}

Example: GET /api/trips/AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA
```

#### Get Seat Availability
```http
GET /api/trips/{tripId}/seats

Example: GET /api/trips/AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA/seats
```

#### Get Boarding/Dropping Points
```http
GET /api/trips/{tripId}/stops

Example: GET /api/trips/AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA/stops
```

---

### 5. Bus APIs

#### Get All Buses
```http
GET /api/buses?pageNumber=1&pageSize=10
```

#### Get Bus Details
```http
GET /api/buses/{busId}

Example: GET /api/buses/77777777-7777-7777-7777-777777777771
```

#### Get Bus Seat Layout
```http
GET /api/buses/{busId}/seats

Example: GET /api/buses/77777777-7777-7777-7777-777777777771/seats
```

#### Get Bus Schedules
```http
GET /api/buses/{busId}/schedules

Example: GET /api/buses/77777777-7777-7777-7777-777777777771/schedules
```

---

### 6. Booking APIs

#### Create Booking
```http
POST /api/bookings
X-User-Token: YOUR_TOKEN_HERE
Content-Type: application/json

{
  "tripId": "AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA",
  "passengers": [
    {
      "name": "John Doe",
      "age": 30,
      "gender": "Male",
      "seatNumber": "L5"
    },
    {
      "name": "Jane Doe",
      "age": 28,
      "gender": "Female",
      "seatNumber": "L6"
    }
  ],
  "boardingPointId": "66666666-6666-6666-6666-666666666661",
  "droppingPointId": "66666666-6666-6666-6666-666666666667",
  "contactEmail": "john.doe@gmail.com",
  "contactPhone": "+919876543210",
  "offerCode": "WELCOME50"
}
```

#### Get Booking Details
```http
GET /api/bookings/{bookingId}

Example: GET /api/bookings/CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC01
```

#### Get Booking by Reference
```http
GET /api/bookings/reference/{bookingReference}

Example: GET /api/bookings/reference/BK20250101ABC123
```

#### Confirm Booking
```http
POST /api/bookings/{bookingId}/confirm
X-User-Token: YOUR_TOKEN_HERE
```

---

### 7. Payment APIs

#### Initiate Payment
```http
POST /api/payments
X-User-Token: YOUR_TOKEN_HERE
Content-Type: application/json

{
  "bookingId": "CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC01",
  "paymentMethod": "Card"
}
```

**Payment Methods:** `Card`, `UPI`, `Wallet`, `NetBanking`

#### Complete Payment
```http
POST /api/payments/{paymentId}/complete
Content-Type: application/json

{
  "transactionId": "TXN123456789"
}
```

#### Get Payment Status
```http
GET /api/payments/{paymentId}
```

#### Get Payment by Booking
```http
GET /api/payments/booking/{bookingId}
```

---

### 8. Cancellation APIs

#### Get Cancellation Policy
```http
GET /api/cancellations/policy
```

#### Calculate Refund Amount
```http
POST /api/cancellations/calculate
Content-Type: application/json

{
  "bookingId": "CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC01"
}
```

#### Create Cancellation
```http
POST /api/cancellations
X-User-Token: YOUR_TOKEN_HERE
Content-Type: application/json

{
  "bookingId": "CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC01",
  "reason": "Change of travel plans"
}
```

#### Get Cancellation Details
```http
GET /api/cancellations/{cancellationId}
```

---

### 9. Refund APIs

#### Get Refund Status
```http
GET /api/refunds/{refundId}
```

#### Get Refund by Cancellation
```http
GET /api/refunds/cancellation/{cancellationId}
```

#### Process Refund (Admin)
```http
POST /api/refunds/{refundId}/process
X-User-Token: ADMIN_TOKEN_HERE
Content-Type: application/json

{
  "transactionId": "REF123456789"
}
```

---

### 10. Offer APIs

#### Get All Active Offers
```http
GET /api/offers
```

#### Validate Offer Code
```http
POST /api/offers/validate
Content-Type: application/json

{
  "offerCode": "WELCOME50",
  "bookingAmount": 1000
}
```

#### Apply Offer
```http
POST /api/offers/apply
Content-Type: application/json

{
  "offerCode": "WELCOME50",
  "bookingId": "CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC01"
}
```

#### Get Offer Details
```http
GET /api/offers/{offerId}
```

---

### 11. Review APIs

#### Get Reviews for Operator
```http
GET /api/reviews/operator/{operatorId}?pageNumber=1&pageSize=10

Example: GET /api/reviews/operator/44444444-4444-4444-4444-444444444441
```

#### Get Reviews for Trip
```http
GET /api/reviews/trip/{tripId}
```

#### Create Review
```http
POST /api/reviews
X-User-Token: YOUR_TOKEN_HERE
Content-Type: application/json

{
  "tripId": "AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA",
  "rating": 5,
  "comment": "Excellent service! Very comfortable journey."
}
```

#### Update Review
```http
PUT /api/reviews/{reviewId}
X-User-Token: YOUR_TOKEN_HERE
Content-Type: application/json

{
  "rating": 4,
  "comment": "Updated review comment"
}
```

#### Delete Review
```http
DELETE /api/reviews/{reviewId}
X-User-Token: YOUR_TOKEN_HERE
```

---

### 12. Notification APIs

#### Get User Notifications
```http
GET /api/notifications?pageNumber=1&pageSize=10
X-User-Token: YOUR_TOKEN_HERE
```

#### Get Unread Notifications Count
```http
GET /api/notifications/unread/count
X-User-Token: YOUR_TOKEN_HERE
```

#### Mark Notification as Read
```http
PATCH /api/notifications/{notificationId}/read
X-User-Token: YOUR_TOKEN_HERE
```

#### Mark All as Read
```http
PATCH /api/notifications/read-all
X-User-Token: YOUR_TOKEN_HERE
```

---

### 13. Operator APIs

> **Note**: All operator endpoints require authentication with an operator account.
> Login with `operator1@redbus.com` / `Password@123` to get the token.

#### Get Operator Dashboard
```http
GET /api/operator/dashboard
X-User-Token: OPERATOR_TOKEN_HERE
```

#### Get Operator Profile
```http
GET /api/operator/profile
X-User-Token: OPERATOR_TOKEN_HERE
```

#### Update Operator Profile
```http
PUT /api/operator/profile
X-User-Token: OPERATOR_TOKEN_HERE
Content-Type: application/json

{
  "companyName": "RedBus Travels Updated",
  "contactEmail": "contact@redbus.com",
  "contactPhone": "+919876543210",
  "address": "123 Main Street",
  "city": "Bangalore",
  "state": "Karnataka"
}
```

#### Get Operator's Buses
```http
GET /api/operator/buses
X-User-Token: OPERATOR_TOKEN_HERE
```

#### Add New Bus
```http
POST /api/operator/buses
X-User-Token: OPERATOR_TOKEN_HERE
Content-Type: application/json

{
  "busNumber": "NEW-001",
  "busType": 0,
  "busCategory": 0,
  "totalSeats": 36,
  "registrationNumber": "KA-01-XX-1234",
  "amenities": ["WiFi", "Charging Point", "Blanket"]
}
```

**Bus Type Values**: `0` = AC, `1` = NonAC
**Bus Category Values**: `0` = Sleeper, `1` = Seater, `2` = SemiSleeper

#### Get Operator's Routes
```http
GET /api/operator/routes
X-User-Token: OPERATOR_TOKEN_HERE
```

#### Create Route
```http
POST /api/operator/routes
X-User-Token: OPERATOR_TOKEN_HERE
Content-Type: application/json

{
  "sourceCity": "Bangalore",
  "destinationCity": "Mumbai",
  "distanceKm": 980,
  "estimatedDurationHours": 14,
  "stops": [
    {
      "stopName": "Hubli",
      "stopOrder": 1,
      "arrivalTimeOffset": "04:00:00",
      "departureTimeOffset": "04:15:00"
    },
    {
      "stopName": "Pune",
      "stopOrder": 2,
      "arrivalTimeOffset": "10:00:00",
      "departureTimeOffset": "10:30:00"
    }
  ]
}
```

#### Get Operator's Schedules
```http
GET /api/operator/schedules
X-User-Token: OPERATOR_TOKEN_HERE
```

#### Create Schedule
```http
POST /api/operator/schedules
X-User-Token: OPERATOR_TOKEN_HERE
Content-Type: application/json

{
  "busId": "77777777-7777-7777-7777-777777777771",
  "routeId": "55555555-5555-5555-5555-555555555551",
  "departureTime": "2025-02-01T21:00:00",
  "arrivalTime": "2025-02-02T05:00:00",
  "baseFare": 850.00,
  "isActive": true,
  "availableDates": ["2025-02-01", "2025-02-02", "2025-02-03"]
}
```

#### Get Schedule Details
```http
GET /api/operator/schedules/{scheduleId}
X-User-Token: OPERATOR_TOKEN_HERE
```

#### Update Schedule
```http
PUT /api/operator/schedules/{scheduleId}
X-User-Token: OPERATOR_TOKEN_HERE
Content-Type: application/json

{
  "departureTime": "2025-02-01T22:00:00",
  "arrivalTime": "2025-02-02T06:00:00",
  "baseFare": 900.00
}
```

#### Delete Schedule
```http
DELETE /api/operator/schedules/{scheduleId}
X-User-Token: OPERATOR_TOKEN_HERE
```

#### Get Operator's Trips
```http
GET /api/operator/trips
X-User-Token: OPERATOR_TOKEN_HERE

# Filter by date
GET /api/operator/trips?date=2025-02-01
```

#### Get Trip Details
```http
GET /api/operator/trips/{tripId}
X-User-Token: OPERATOR_TOKEN_HERE
```

#### Update Trip Status
```http
PATCH /api/operator/trips/{tripId}/status
X-User-Token: OPERATOR_TOKEN_HERE
Content-Type: application/json

{
  "currentStatus": 1
}
```

**Trip Status Values**: `0` = Scheduled, `1` = InTransit, `2` = Completed, `3` = Cancelled

#### Cancel Trip
```http
POST /api/operator/trips/{tripId}/cancel
X-User-Token: OPERATOR_TOKEN_HERE
Content-Type: application/json

{
  "reason": "Weather conditions"
}
```

#### Get Trip Bookings
```http
GET /api/operator/trips/{tripId}/bookings
X-User-Token: OPERATOR_TOKEN_HERE
```

#### Get Trip Passengers
```http
GET /api/operator/trips/{tripId}/passengers
X-User-Token: OPERATOR_TOKEN_HERE
```

#### Get Operator's Bookings
```http
GET /api/operator/bookings?pageNumber=1&pageSize=10
X-User-Token: OPERATOR_TOKEN_HERE
```

#### Get Revenue Statistics
```http
GET /api/operator/revenue?startDate=2025-01-01&endDate=2025-01-31&groupBy=day
X-User-Token: OPERATOR_TOKEN_HERE
```

#### Export Revenue Report
```http
GET /api/operator/revenue/export?startDate=2025-01-01&endDate=2025-01-31&format=csv
X-User-Token: OPERATOR_TOKEN_HERE
```

#### Get Operator Reviews
```http
GET /api/operator/reviews?pageNumber=1&pageSize=10
X-User-Token: OPERATOR_TOKEN_HERE
```

#### Get Review Statistics
```http
GET /api/operator/reviews/stats
X-User-Token: OPERATOR_TOKEN_HERE
```

---

### 14. Admin APIs

> **Note**: All admin endpoints require authentication with an admin account.
> Login with `admin@busbooking.com` / `Password@123` to get the token.

#### Get Dashboard Stats
```http
GET /api/admin/dashboard
X-User-Token: ADMIN_TOKEN_HERE
```

#### Get Analytics
```http
GET /api/admin/analytics?metric=bookings&startDate=2025-01-01&endDate=2025-01-31
X-User-Token: ADMIN_TOKEN_HERE
```

**Metric Values**: `bookings`, `revenue`, `users`, `trips`

#### Get Reports
```http
GET /api/admin/reports?reportType=bookings&startDate=2025-01-01&endDate=2025-01-31
X-User-Token: ADMIN_TOKEN_HERE
```

**Report Types**: `bookings`, `revenue`, `operators`, `users`

#### Get All Users
```http
GET /api/admin/users?pageNumber=1&pageSize=10&role=Passenger
X-User-Token: ADMIN_TOKEN_HERE
```

#### Get User Details
```http
GET /api/admin/users/{userId}
X-User-Token: ADMIN_TOKEN_HERE
```

#### Update User Status
```http
PATCH /api/admin/users/{userId}/status
X-User-Token: ADMIN_TOKEN_HERE
Content-Type: application/json

{
  "isActive": false
}
```

#### Delete User
```http
DELETE /api/admin/users/{userId}
X-User-Token: ADMIN_TOKEN_HERE
```

#### Get User Statistics
```http
GET /api/admin/users/statistics
X-User-Token: ADMIN_TOKEN_HERE
```

#### Get All Operators
```http
GET /api/admin/operators?pageNumber=1&pageSize=10
X-User-Token: ADMIN_TOKEN_HERE
```

#### Get Operator Details
```http
GET /api/admin/operators/{operatorId}
X-User-Token: ADMIN_TOKEN_HERE
```

#### Get Pending Operator Approvals
```http
GET /api/admin/operators/pending
X-User-Token: ADMIN_TOKEN_HERE
```

#### Approve Operator
```http
POST /api/admin/operators/approve
X-User-Token: ADMIN_TOKEN_HERE
Content-Type: application/json

{
  "operatorId": "44444444-4444-4444-4444-444444444441"
}
```

#### Update Operator Status
```http
PATCH /api/admin/operators/{operatorId}/status
X-User-Token: ADMIN_TOKEN_HERE
Content-Type: application/json

{
  "isApproved": false
}
```

#### Delete Operator
```http
DELETE /api/admin/operators/{operatorId}
X-User-Token: ADMIN_TOKEN_HERE
```

#### Get All Bookings
```http
GET /api/admin/bookings?pageNumber=1&pageSize=10&status=Confirmed
X-User-Token: ADMIN_TOKEN_HERE
```

#### Get Booking Statistics
```http
GET /api/admin/bookings/statistics
X-User-Token: ADMIN_TOKEN_HERE
```

#### Modify Booking
```http
PATCH /api/admin/bookings/{bookingId}
X-User-Token: ADMIN_TOKEN_HERE
Content-Type: application/json

{
  "bookingStatus": 2
}
```

**Booking Status Values**: `0` = Pending, `1` = Confirmed, `2` = Cancelled, `3` = Completed

#### Get All Payments
```http
GET /api/admin/payments?pageNumber=1&pageSize=10&status=Completed
X-User-Token: ADMIN_TOKEN_HERE
```

#### Get Failed Payments
```http
GET /api/admin/payments/failed
X-User-Token: ADMIN_TOKEN_HERE
```

#### Get Payment Reconciliation
```http
GET /api/admin/payments/reconcile?date=2025-01-30
X-User-Token: ADMIN_TOKEN_HERE
```

#### Approve Refund
```http
POST /api/admin/refunds/{refundId}/approve
X-User-Token: ADMIN_TOKEN_HERE
Content-Type: application/json

{
  "notes": "Approved by admin"
}
```

#### Reject Refund
```http
POST /api/admin/refunds/{refundId}/reject
X-User-Token: ADMIN_TOKEN_HERE
Content-Type: application/json

{
  "reason": "Invalid refund request"
}
```

#### Get All Offers
```http
GET /api/admin/offers?pageNumber=1&pageSize=10
X-User-Token: ADMIN_TOKEN_HERE
```

#### Create Offer
```http
POST /api/admin/offers
X-User-Token: ADMIN_TOKEN_HERE
Content-Type: application/json

{
  "offerCode": "NEWOFFER25",
  "description": "New Year Special - 25% off",
  "discountType": 0,
  "discountValue": 25,
  "minBookingAmount": 500,
  "maxDiscount": 200,
  "validFrom": "2025-01-01",
  "validTo": "2025-03-31",
  "usageLimit": 1000,
  "isActive": true
}
```

**Discount Type Values**: `0` = Percentage, `1` = Flat

#### Update Offer
```http
PUT /api/admin/offers/{offerId}
X-User-Token: ADMIN_TOKEN_HERE
Content-Type: application/json

{
  "description": "Updated description",
  "discountValue": 30,
  "isActive": false
}
```

#### Delete Offer
```http
DELETE /api/admin/offers/{offerId}
X-User-Token: ADMIN_TOKEN_HERE
```

---

## ?? Testing with Swagger UI

1. Open Swagger UI at `http://localhost:5129/swagger`

2. **For authenticated endpoints:**
   - First call `POST /api/auth/login` with test credentials
   - Copy the `token` from the response
   - For each authenticated request, add header: `X-User-Token: YOUR_TOKEN`

### Testing Different Roles

#### As Passenger:
```bash
# Login
POST /api/auth/login
{ "email": "john.doe@gmail.com", "password": "Password@123" }

# Copy token and use in:
GET /api/users/profile
POST /api/bookings
GET /api/users/profile/bookings
```

#### As Operator:
```bash
# Login
POST /api/auth/login
{ "email": "operator1@redbus.com", "password": "Password@123" }

# Copy token and use in:
GET /api/operator/dashboard
GET /api/operator/buses
POST /api/operator/schedules
GET /api/operator/revenue
```

#### As Admin:
```bash
# Login
POST /api/auth/login
{ "email": "admin@busbooking.com", "password": "Password@123" }

# Copy token and use in:
GET /api/admin/dashboard
GET /api/admin/users
GET /api/admin/operators/pending
POST /api/admin/offers
```

### Common Errors

| Error | Cause | Solution |
|-------|-------|----------|
| 401 Unauthorized | Missing or invalid token | Login again to get a new token |
| 401 "Not authorized as operator" | Using non-operator account | Login with operator credentials |
| 401 "Admin access required" | Using non-admin account | Login with admin credentials |
| 404 Not Found | Resource doesn't exist | Check the ID in your request |

## ?? Sample Data IDs (from Seed Data)

### User IDs
| User | ID |
|------|----|
| Admin | 11111111-1111-1111-1111-111111111111 |
| Operator (RedBus) | 22222222-2222-2222-2222-222222222221 |
| Passenger (John) | 33333333-3333-3333-3333-333333333331 |

### Operator IDs
| Operator | ID |
|----------|----|
| RedBus Travels | 44444444-4444-4444-4444-444444444441 |
| VRL Travels | 44444444-4444-4444-4444-444444444442 |
| SRS Travels | 44444444-4444-4444-4444-444444444443 |

### Route IDs
| Route | ID |
|-------|----|
| Bangalore ? Chennai | 55555555-5555-5555-5555-555555555551 |
| Bangalore ? Hyderabad | 55555555-5555-5555-5555-555555555552 |
| Bangalore ? Mysore | 55555555-5555-5555-5555-555555555560 |

### Bus IDs
| Bus | ID |
|-----|----|
| RB-001 (RedBus AC Sleeper) | 77777777-7777-7777-7777-777777777771 |
| VRL-101 (VRL AC Sleeper) | 77777777-7777-7777-7777-777777777774 |
| KSRTC-301 (KSRTC Semi-Sleeper) | 77777777-7777-7777-7777-777777777778 |

### Sample Trip IDs
| Trip | ID |
|------|----|
| Sample Trip 1 | AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA |
| Sample Trip 2 | AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAB |

### Sample Booking IDs
| Booking | ID |
|---------|----|
| Booking 1 | CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC01 |
| Booking 2 | CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC02 |

---

## ?? Project Structure

```
bus-ticket-api/
??? Controllers/
?   ??? AuthController.cs
?   ??? UsersController.cs
?   ??? SearchController.cs
?   ??? TripsController.cs
?   ??? BusesController.cs
?   ??? BookingsController.cs
?   ??? PaymentsController.cs
?   ??? CancellationsController.cs
?   ??? RefundsController.cs
?   ??? OffersController.cs
?   ??? ReviewsController.cs
?   ??? NotificationsController.cs
?   ??? OperatorController.cs
?   ??? AdminController.cs
??? Data/
?   ??? AppDbContext.cs
?   ??? SeedData.sql
??? DTOs/
?   ??? Auth/
?   ??? User/
?   ??? Search/
?   ??? Trip/
?   ??? Bus/
?   ??? Booking/
?   ??? Payment/
?   ??? Cancellation/
?   ??? Refund/
?   ??? Offer/
?   ??? Review/
?   ??? Notification/
?   ??? Operator/
?   ??? Admin/
?   ??? Common/
??? Models/
?   ??? User.cs
?   ??? Bus.cs
?   ??? Route.cs
?   ??? Schedule.cs
?   ??? Trip.cs
?   ??? Booking.cs
?   ??? Payment.cs
?   ??? Cancellation.cs
?   ??? Review.cs
?   ??? Offer.cs
?   ??? Notification.cs
?   ??? Enums.cs
??? Program.cs
??? appsettings.json
??? README.md
```

---

## ?? Common Response Format

All API responses follow this format:

```json
{
  "success": true,
  "message": "Operation successful",
  "data": { ... },
  "errors": null
}
```

### Error Response
```json
{
  "success": false,
  "message": "Error description",
  "data": null,
  "errors": ["Detailed error 1", "Detailed error 2"]
}
```

---

## ?? License

This project is licensed under the MIT License.

## ????? Author

Alan John - [GitHub](https://github.com/alanjohnck)
