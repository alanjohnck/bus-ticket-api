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

This API uses a simple token-based authentication system.

### How Authentication Works

1. **Login** to get a token
2. **Include the token** in subsequent requests using the `X-User-Token` header

### Test Credentials (from seed data)

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@busbooking.com | Password@123 |
| Operator | operator1@redbus.com | Password@123 |
| Passenger | john.doe@gmail.com | Password@123 |

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

#### Get Operator Profile
```http
GET /api/operator/profile
X-User-Token: OPERATOR_TOKEN_HERE
```

#### Get Operator's Buses
```http
GET /api/operator/buses?pageNumber=1&pageSize=10
X-User-Token: OPERATOR_TOKEN_HERE
```

#### Add New Bus
```http
POST /api/operator/buses
X-User-Token: OPERATOR_TOKEN_HERE
Content-Type: application/json

{
  "busNumber": "NEW-001",
  "busType": "AC",
  "busCategory": "Sleeper",
  "totalSeats": 36,
  "registrationNumber": "KA-01-XX-1234",
  "amenities": ["WiFi", "Charging Point", "Blanket"]
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
  "departureTime": "21:00:00",
  "arrivalTime": "03:00:00",
  "baseFare": 850.00
}
```

#### Get Operator's Bookings
```http
GET /api/operator/bookings?pageNumber=1&pageSize=10
X-User-Token: OPERATOR_TOKEN_HERE
```

---

### 14. Admin APIs

#### Get Dashboard Stats
```http
GET /api/admin/dashboard
X-User-Token: ADMIN_TOKEN_HERE
```

#### Get All Users
```http
GET /api/admin/users?pageNumber=1&pageSize=10
X-User-Token: ADMIN_TOKEN_HERE
```

#### Get All Operators
```http
GET /api/admin/operators?pageNumber=1&pageSize=10
X-User-Token: ADMIN_TOKEN_HERE
```

#### Approve Operator
```http
POST /api/admin/operators/{operatorId}/approve
X-User-Token: ADMIN_TOKEN_HERE
```

#### Reject Operator
```http
POST /api/admin/operators/{operatorId}/reject
X-User-Token: ADMIN_TOKEN_HERE
Content-Type: application/json

{
  "reason": "Incomplete documentation"
}
```

#### Get All Bookings
```http
GET /api/admin/bookings?pageNumber=1&pageSize=10
X-User-Token: ADMIN_TOKEN_HERE
```

#### Create Offer (Admin)
```http
POST /api/admin/offers
X-User-Token: ADMIN_TOKEN_HERE
Content-Type: application/json

{
  "offerCode": "NEWOFFER25",
  "description": "New Year Special - 25% off",
  "discountType": "Percentage",
  "discountValue": 25,
  "minBookingAmount": 500,
  "maxDiscount": 200,
  "validFrom": "2025-01-01",
  "validTo": "2025-03-31",
  "usageLimit": 1000
}
```

---

## ?? Testing with Swagger UI

1. Open Swagger UI at `http://localhost:5129/swagger`
2. **For authenticated endpoints:**
   - First call `POST /api/auth/login` with test credentials
   - Copy the `token` from the response
   - For each authenticated request, add header: `X-User-Token: YOUR_TOKEN`

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
