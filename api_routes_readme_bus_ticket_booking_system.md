# Bus Ticket Booking System – API Routes Documentation

This document describes all backend API routes for the Bus Ticket Booking System. The APIs are grouped by modules for clarity and ease of development.

---

## Base URL
```
/api
```

---

## 1. Authentication

| Method | Endpoint | Description |
|------|---------|------------|
| POST | /auth/register | Register a new user |
| POST | /auth/login | User login |
| POST | /auth/logout | Logout user |
| POST | /auth/verify-email | Verify email using OTP / link |
| POST | /auth/resend-verification | Resend verification email |

---

## 2. User Profile

| Method | Endpoint | Description |
|------|---------|------------|
| GET | /users/profile | Get current user profile |
| PUT | /users/profile | Update profile details |
| PATCH | /users/profile/password | Change password |
| DELETE | /users/profile | Delete user account |
| GET | /users/profile/bookings | Get booking history |
| GET | /users/profile/reviews | Get user reviews |

---

## 3. Bus Search & Availability

| Method | Endpoint | Description |
|------|---------|------------|
| GET | /search/buses | Search buses by source, destination, date |
| GET | /search/routes | Get all routes |
| GET | /search/cities | Get list of cities |
| GET | /search/popular-routes | Get popular routes |
| GET | /search/autocomplete | City autocomplete search |

---

## 4. Bus & Trip Details

### Bus Details

| Method | Endpoint | Description |
|------|---------|------------|
| GET | /buses/:busId | Get bus details |
| GET | /buses/:busId/amenities | Get amenities |
| GET | /buses/:busId/reviews | Get bus reviews |
| GET | /buses/:busId/seat-layout | Get seat layout |

### Trip Details

| Method | Endpoint | Description |
|------|---------|------------|
| GET | /trips/:tripId | Get trip details |
| GET | /trips/:tripId/availability | Seat availability |
| GET | /trips/:tripId/fare | Fare details |
| GET | /trips/:tripId/stops | Boarding & dropping points |

---

## 5. Booking Management

| Method | Endpoint | Description |
|------|---------|------------|
| POST | /bookings | Create new booking |
| POST | /bookings/hold-seats | Temporarily hold seats |
| DELETE | /bookings/release-seats | Release held seats |
| GET | /bookings | Get user bookings |
| GET | /bookings/:bookingId | Booking details |
| GET | /bookings/:bookingId/ticket | Download e-ticket |
| PATCH | /bookings/:bookingId | Modify booking |
| DELETE | /bookings/:bookingId | Cancel booking |
| GET | /bookings/reference/:ref | Get booking by reference |
| POST | /bookings/:bookingId/resend | Resend confirmation |

---

## 6. Payments & Refunds

### Payments

| Method | Endpoint | Description |
|------|---------|------------|
| POST | /payments/initiate | Initiate payment |
| POST | /payments/verify | Verify payment |
| GET | /payments/:paymentId | Payment details |
| POST | /payments/callback | Gateway callback |
| GET | /payments/methods | Available methods |

### Refunds

| Method | Endpoint | Description |
|------|---------|------------|
| POST | /refunds | Request refund |
| GET | /refunds/:refundId | Refund status |
| GET | /refunds/booking/:bookingId | Refund by booking |

---

## 7. Cancellations

| Method | Endpoint | Description |
|------|---------|------------|
| POST | /cancellations | Cancel booking |
| GET | /cancellations/:cancellationId | Cancellation details |
| GET | /cancellations/policy | Cancellation policy |
| POST | /cancellations/calculate | Calculate refund |

---

## 8. Reviews & Ratings

| Method | Endpoint | Description |
|------|---------|------------|
| POST | /reviews | Submit review |
| GET | /reviews/trip/:tripId | Trip reviews |
| GET | /reviews/operator/:operatorId | Operator reviews |
| GET | /reviews/:reviewId | Review details |
| PUT | /reviews/:reviewId | Update review |
| DELETE | /reviews/:reviewId | Delete review |

---

## 9. Offers & Discounts

| Method | Endpoint | Description |
|------|---------|------------|
| GET | /offers | Get active offers |
| GET | /offers/:offerId | Offer details |
| POST | /offers/validate | Validate coupon |
| GET | /offers/applicable | Applicable offers |

---

## 10. Notifications

| Method | Endpoint | Description |
|------|---------|------------|
| GET | /notifications | Get notifications |
| GET | /notifications/:notificationId | Notification details |
| PATCH | /notifications/:notificationId/read | Mark as read |
| PATCH | /notifications/read-all | Mark all as read |
| DELETE | /notifications/:notificationId | Delete notification |
| GET | /notifications/unread-count | Unread count |

---

## 11. Operator APIs

Includes fleet management, routes, schedules, trips, bookings, revenue, and review management.

Base Path:
```
/api/operator
```

---

## 12. Admin APIs

Includes user management, operator approval, reports, analytics, disputes, payments, and offers.

Base Path:
```
/api/admin
```

---

## Notes
- All protected routes require JWT authentication
- Role-based access control is enforced (Passenger, Operator, Admin)
- Responses follow REST standards
- Errors return proper HTTP status codes

---

**End of API Routes Documentation**

