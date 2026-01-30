-- =============================================
-- Bus Ticket Booking System - Database Seed Data
-- =============================================
-- Run this script after database migrations to populate initial data
-- Ensure the database schema is created before running this script

-- =============================================
-- 1. USERS TABLE
-- =============================================
-- Password hash is for 'Password@123' (SHA256)
DECLARE @PasswordHash NVARCHAR(100) = 'sQnzu7wkTrgkQZF+0G1hi5AI3Qmj8AUz7DRrY9WBRU0='

-- Admin Users
INSERT INTO Users (UserId, Email, PasswordHash, FirstName, LastName, PhoneNumber, DateOfBirth, Gender, Role, IsVerified, CreatedAt, UpdatedAt)
VALUES 
    ('11111111-1111-1111-1111-111111111111', 'admin@busbooking.com', @PasswordHash, 'System', 'Admin', '+919876543210', '1985-01-15', 'Male', 2, 1, GETUTCDATE(), GETUTCDATE()),
    ('11111111-1111-1111-1111-111111111112', 'superadmin@busbooking.com', @PasswordHash, 'Super', 'Admin', '+919876543211', '1980-05-20', 'Male', 2, 1, GETUTCDATE(), GETUTCDATE());

-- Operator Users
INSERT INTO Users (UserId, Email, PasswordHash, FirstName, LastName, PhoneNumber, DateOfBirth, Gender, Role, IsVerified, CreatedAt, UpdatedAt)
VALUES 
    ('22222222-2222-2222-2222-222222222221', 'operator1@redbus.com', @PasswordHash, 'Rajesh', 'Kumar', '+919876543220', '1978-03-10', 'Male', 1, 1, GETUTCDATE(), GETUTCDATE()),
    ('22222222-2222-2222-2222-222222222222', 'operator2@vrl.com', @PasswordHash, 'Suresh', 'Patel', '+919876543221', '1982-07-25', 'Male', 1, 1, GETUTCDATE(), GETUTCDATE()),
    ('22222222-2222-2222-2222-222222222223', 'operator3@srs.com', @PasswordHash, 'Anita', 'Sharma', '+919876543222', '1985-11-30', 'Female', 1, 1, GETUTCDATE(), GETUTCDATE()),
    ('22222222-2222-2222-2222-222222222224', 'operator4@ksrtc.com', @PasswordHash, 'Vikram', 'Reddy', '+919876543223', '1975-09-12', 'Male', 1, 1, GETUTCDATE(), GETUTCDATE()),
    ('22222222-2222-2222-2222-222222222225', 'operator5@orange.com', @PasswordHash, 'Priya', 'Nair', '+919876543224', '1988-04-18', 'Female', 1, 1, GETUTCDATE(), GETUTCDATE());

-- Passenger Users
INSERT INTO Users (UserId, Email, PasswordHash, FirstName, LastName, PhoneNumber, DateOfBirth, Gender, Role, IsVerified, CreatedAt, UpdatedAt)
VALUES 
    ('33333333-3333-3333-3333-333333333331', 'john.doe@gmail.com', @PasswordHash, 'John', 'Doe', '+919876543230', '1990-06-15', 'Male', 0, 1, GETUTCDATE(), GETUTCDATE()),
    ('33333333-3333-3333-3333-333333333332', 'jane.smith@gmail.com', @PasswordHash, 'Jane', 'Smith', '+919876543231', '1992-08-22', 'Female', 0, 1, GETUTCDATE(), GETUTCDATE()),
    ('33333333-3333-3333-3333-333333333333', 'mike.wilson@yahoo.com', @PasswordHash, 'Mike', 'Wilson', '+919876543232', '1988-12-05', 'Male', 0, 1, GETUTCDATE(), GETUTCDATE()),
    ('33333333-3333-3333-3333-333333333334', 'sarah.johnson@outlook.com', @PasswordHash, 'Sarah', 'Johnson', '+919876543233', '1995-02-28', 'Female', 0, 1, GETUTCDATE(), GETUTCDATE()),
    ('33333333-3333-3333-3333-333333333335', 'david.brown@gmail.com', @PasswordHash, 'David', 'Brown', '+919876543234', '1987-10-10', 'Male', 0, 1, GETUTCDATE(), GETUTCDATE()),
    ('33333333-3333-3333-3333-333333333336', 'emily.davis@gmail.com', @PasswordHash, 'Emily', 'Davis', '+919876543235', '1993-04-17', 'Female', 0, 1, GETUTCDATE(), GETUTCDATE()),
    ('33333333-3333-3333-3333-333333333337', 'robert.taylor@hotmail.com', @PasswordHash, 'Robert', 'Taylor', '+919876543236', '1985-07-23', 'Male', 0, 1, GETUTCDATE(), GETUTCDATE()),
    ('33333333-3333-3333-3333-333333333338', 'lisa.anderson@gmail.com', @PasswordHash, 'Lisa', 'Anderson', '+919876543237', '1991-11-08', 'Female', 0, 0, GETUTCDATE(), GETUTCDATE()),
    ('33333333-3333-3333-3333-333333333339', 'james.thomas@yahoo.com', @PasswordHash, 'James', 'Thomas', '+919876543238', '1989-01-30', 'Male', 0, 1, GETUTCDATE(), GETUTCDATE()),
    ('33333333-3333-3333-3333-333333333340', 'amanda.white@gmail.com', @PasswordHash, 'Amanda', 'White', '+919876543239', '1994-09-14', 'Female', 0, 1, GETUTCDATE(), GETUTCDATE());

-- =============================================
-- 2. BUS OPERATORS TABLE
-- =============================================
INSERT INTO BusOperators (OperatorId, UserId, CompanyName, LicenseNumber, ContactEmail, ContactPhone, Address, City, State, Rating, IsApproved, CreatedAt)
VALUES 
    ('44444444-4444-4444-4444-444444444441', '22222222-2222-2222-2222-222222222221', 'RedBus Travels', 'RB-2020-001', 'contact@redbus.com', '+918001234567', '123 MG Road, Koramangala', 'Bangalore', 'Karnataka', 4.5, 1, GETUTCDATE()),
    ('44444444-4444-4444-4444-444444444442', '22222222-2222-2222-2222-222222222222', 'VRL Travels', 'VRL-2019-002', 'support@vrl.com', '+918001234568', '456 Station Road', 'Hubli', 'Karnataka', 4.3, 1, GETUTCDATE()),
    ('44444444-4444-4444-4444-444444444443', '22222222-2222-2222-2222-222222222223', 'SRS Travels', 'SRS-2018-003', 'info@srs.com', '+918001234569', '789 Poonamallee High Road', 'Chennai', 'Tamil Nadu', 4.2, 1, GETUTCDATE()),
    ('44444444-4444-4444-4444-444444444444', '22222222-2222-2222-2222-222222222224', 'KSRTC Premium', 'KSRTC-2021-004', 'ksrtc@karnataka.gov.in', '+918001234570', 'Central Bus Station', 'Bangalore', 'Karnataka', 4.0, 1, GETUTCDATE()),
    ('44444444-4444-4444-4444-444444444445', '22222222-2222-2222-2222-222222222225', 'Orange Travels', 'OT-2020-005', 'book@orange.com', '+918001234571', '321 Jubilee Hills', 'Hyderabad', 'Telangana', 4.4, 0, GETUTCDATE());

-- =============================================
-- 3. ROUTES TABLE
-- =============================================
INSERT INTO Routes (RouteId, SourceCity, DestinationCity, DistanceKm, EstimatedDurationHours, CreatedAt)
VALUES 
    ('55555555-5555-5555-5555-555555555551', 'Bangalore', 'Chennai', 350, 6.0, GETUTCDATE()),
    ('55555555-5555-5555-5555-555555555552', 'Bangalore', 'Hyderabad', 570, 9.0, GETUTCDATE()),
    ('55555555-5555-5555-5555-555555555553', 'Bangalore', 'Mumbai', 980, 14.0, GETUTCDATE()),
    ('55555555-5555-5555-5555-555555555554', 'Chennai', 'Hyderabad', 630, 10.0, GETUTCDATE()),
    ('55555555-5555-5555-5555-555555555555', 'Mumbai', 'Pune', 150, 3.5, GETUTCDATE()),
    ('55555555-5555-5555-5555-555555555556', 'Delhi', 'Jaipur', 280, 5.0, GETUTCDATE()),
    ('55555555-5555-5555-5555-555555555557', 'Bangalore', 'Goa', 560, 10.0, GETUTCDATE()),
    ('55555555-5555-5555-5555-555555555558', 'Hyderabad', 'Vijayawada', 275, 5.0, GETUTCDATE()),
    ('55555555-5555-5555-5555-555555555559', 'Chennai', 'Coimbatore', 500, 8.0, GETUTCDATE()),
    ('55555555-5555-5555-5555-555555555560', 'Bangalore', 'Mysore', 150, 3.0, GETUTCDATE());

-- =============================================
-- 4. STOPS TABLE
-- =============================================
-- Bangalore to Chennai Route Stops
INSERT INTO Stops (StopId, RouteId, StopName, StopOrder, ArrivalTimeOffset, DepartureTimeOffset)
VALUES 
    ('66666666-6666-6666-6666-666666666661', '55555555-5555-5555-5555-555555555551', 'Bangalore - Majestic Bus Stand', 1, '00:00:00', '00:00:00'),
    ('66666666-6666-6666-6666-666666666662', '55555555-5555-5555-5555-555555555551', 'Electronic City', 2, '00:30:00', '00:35:00'),
    ('66666666-6666-6666-6666-666666666663', '55555555-5555-5555-5555-555555555551', 'Hosur', 3, '01:00:00', '01:10:00'),
    ('66666666-6666-6666-6666-666666666664', '55555555-5555-5555-5555-555555555551', 'Krishnagiri', 4, '02:00:00', '02:15:00'),
    ('66666666-6666-6666-6666-666666666665', '55555555-5555-5555-5555-555555555551', 'Vellore', 5, '03:30:00', '03:45:00'),
    ('66666666-6666-6666-6666-666666666666', '55555555-5555-5555-5555-555555555551', 'Kanchipuram', 6, '05:00:00', '05:10:00'),
    ('66666666-6666-6666-6666-666666666667', '55555555-5555-5555-5555-555555555551', 'Chennai - CMBT', 7, '06:00:00', '06:00:00');

-- Bangalore to Hyderabad Route Stops
INSERT INTO Stops (StopId, RouteId, StopName, StopOrder, ArrivalTimeOffset, DepartureTimeOffset)
VALUES 
    ('66666666-6666-6666-6666-666666666671', '55555555-5555-5555-5555-555555555552', 'Bangalore - Majestic', 1, '00:00:00', '00:00:00'),
    ('66666666-6666-6666-6666-666666666672', '55555555-5555-5555-5555-555555555552', 'Anantapur', 2, '03:30:00', '03:45:00'),
    ('66666666-6666-6666-6666-666666666673', '55555555-5555-5555-5555-555555555552', 'Kurnool', 3, '05:30:00', '05:45:00'),
    ('66666666-6666-6666-6666-666666666674', '55555555-5555-5555-5555-555555555552', 'Mahbubnagar', 4, '07:30:00', '07:45:00'),
    ('66666666-6666-6666-6666-666666666675', '55555555-5555-5555-5555-555555555552', 'Hyderabad - MGBS', 5, '09:00:00', '09:00:00');

-- Bangalore to Mysore Route Stops
INSERT INTO Stops (StopId, RouteId, StopName, StopOrder, ArrivalTimeOffset, DepartureTimeOffset)
VALUES 
    ('66666666-6666-6666-6666-666666666681', '55555555-5555-5555-5555-555555555560', 'Bangalore - Satellite Bus Stand', 1, '00:00:00', '00:00:00'),
    ('66666666-6666-6666-6666-666666666682', '55555555-5555-5555-5555-555555555560', 'Ramanagara', 2, '00:45:00', '00:50:00'),
    ('66666666-6666-6666-6666-666666666683', '55555555-5555-5555-5555-555555555560', 'Channapatna', 3, '01:15:00', '01:20:00'),
    ('66666666-6666-6666-6666-666666666684', '55555555-5555-5555-5555-555555555560', 'Mandya', 4, '02:00:00', '02:10:00'),
    ('66666666-6666-6666-6666-666666666685', '55555555-5555-5555-5555-555555555560', 'Mysore - Central Bus Stand', 5, '03:00:00', '03:00:00');

-- =============================================
-- 5. BUSES TABLE
-- =============================================
INSERT INTO Buses (BusId, OperatorId, BusNumber, BusType, BusCategory, TotalSeats, AmenitiesJson, RegistrationNumber, IsActive, CreatedAt)
VALUES 
    -- RedBus Travels Buses
    ('77777777-7777-7777-7777-777777777771', '44444444-4444-4444-4444-444444444441', 'RB-001', 0, 0, 36, '["WiFi","Charging Point","Water Bottle","Blanket","Pillow"]', 'KA-01-AB-1234', 1, GETUTCDATE()),
    ('77777777-7777-7777-7777-777777777772', '44444444-4444-4444-4444-444444444441', 'RB-002', 0, 2, 40, '["WiFi","Charging Point","Water Bottle"]', 'KA-01-AB-1235', 1, GETUTCDATE()),
    ('77777777-7777-7777-7777-777777777773', '44444444-4444-4444-4444-444444444441', 'RB-003', 1, 1, 52, '["Water Bottle"]', 'KA-01-AB-1236', 1, GETUTCDATE()),
    
    -- VRL Travels Buses
    ('77777777-7777-7777-7777-777777777774', '44444444-4444-4444-4444-444444444442', 'VRL-101', 0, 0, 30, '["WiFi","Charging Point","Water Bottle","Blanket","TV","Snacks"]', 'KA-25-CD-5678', 1, GETUTCDATE()),
    ('77777777-7777-7777-7777-777777777775', '44444444-4444-4444-4444-444444444442', 'VRL-102', 0, 2, 45, '["WiFi","Charging Point","Water Bottle","Blanket"]', 'KA-25-CD-5679', 1, GETUTCDATE()),
    
    -- SRS Travels Buses
    ('77777777-7777-7777-7777-777777777776', '44444444-4444-4444-4444-444444444443', 'SRS-201', 0, 0, 32, '["WiFi","Charging Point","Water Bottle","Blanket","Pillow","Reading Light"]', 'TN-01-EF-9012', 1, GETUTCDATE()),
    ('77777777-7777-7777-7777-777777777777', '44444444-4444-4444-4444-444444444443', 'SRS-202', 1, 1, 48, '["Water Bottle","Fan"]', 'TN-01-EF-9013', 1, GETUTCDATE()),
    
    -- KSRTC Premium Buses
    ('77777777-7777-7777-7777-777777777778', '44444444-4444-4444-4444-444444444444', 'KSRTC-301', 0, 2, 44, '["WiFi","Charging Point","Water Bottle"]', 'KA-01-G-0001', 1, GETUTCDATE()),
    ('77777777-7777-7777-7777-777777777779', '44444444-4444-4444-4444-444444444444', 'KSRTC-302', 1, 1, 54, '["Water Bottle"]', 'KA-01-G-0002', 1, GETUTCDATE()),
    ('77777777-7777-7777-7777-777777777780', '44444444-4444-4444-4444-444444444444', 'KSRTC-303', 0, 0, 28, '["WiFi","Charging Point","Water Bottle","Blanket","Pillow","TV"]', 'KA-01-G-0003', 1, GETUTCDATE());

-- =============================================
-- 6. SEAT LAYOUTS TABLE
-- =============================================
-- Seat layout for RB-001 (36 seater sleeper - 18 lower + 18 upper)
-- Lower Deck
INSERT INTO SeatLayouts (LayoutId, BusId, SeatNumber, SeatType, Deck, PositionX, PositionY, IsAvailable)
VALUES 
    ('88888888-8888-8888-8888-888888888801', '77777777-7777-7777-7777-777777777771', 'L1', 0, 0, 0, 0, 1),
    ('88888888-8888-8888-8888-888888888802', '77777777-7777-7777-7777-777777777771', 'L2', 0, 0, 0, 1, 1),
    ('88888888-8888-8888-8888-888888888803', '77777777-7777-7777-7777-777777777771', 'L3', 0, 0, 2, 0, 1),
    ('88888888-8888-8888-8888-888888888804', '77777777-7777-7777-7777-777777777771', 'L4', 0, 0, 2, 1, 1),
    ('88888888-8888-8888-8888-888888888805', '77777777-7777-7777-7777-777777777771', 'L5', 0, 0, 0, 2, 1),
    ('88888888-8888-8888-8888-888888888806', '77777777-7777-7777-7777-777777777771', 'L6', 0, 0, 0, 3, 1),
    ('88888888-8888-8888-8888-888888888807', '77777777-7777-7777-7777-777777777771', 'L7', 0, 0, 2, 2, 1),
    ('88888888-8888-8888-8888-888888888808', '77777777-7777-7777-7777-777777777771', 'L8', 0, 0, 2, 3, 1),
    ('88888888-8888-8888-8888-888888888809', '77777777-7777-7777-7777-777777777771', 'L9', 0, 0, 0, 4, 1),
    ('88888888-8888-8888-8888-888888888810', '77777777-7777-7777-7777-777777777771', 'L10', 0, 0, 0, 5, 1),
    ('88888888-8888-8888-8888-888888888811', '77777777-7777-7777-7777-777777777771', 'L11', 0, 0, 2, 4, 1),
    ('88888888-8888-8888-8888-888888888812', '77777777-7777-7777-7777-777777777771', 'L12', 0, 0, 2, 5, 1),
    ('88888888-8888-8888-8888-888888888813', '77777777-7777-7777-7777-777777777771', 'L13', 0, 0, 0, 6, 1),
    ('88888888-8888-8888-8888-888888888814', '77777777-7777-7777-7777-777777777771', 'L14', 0, 0, 0, 7, 1),
    ('88888888-8888-8888-8888-888888888815', '77777777-7777-7777-7777-777777777771', 'L15', 0, 0, 2, 6, 1),
    ('88888888-8888-8888-8888-888888888816', '77777777-7777-7777-7777-777777777771', 'L16', 0, 0, 2, 7, 1),
    ('88888888-8888-8888-8888-888888888817', '77777777-7777-7777-7777-777777777771', 'L17', 0, 0, 0, 8, 1),
    ('88888888-8888-8888-8888-888888888818', '77777777-7777-7777-7777-777777777771', 'L18', 0, 0, 2, 8, 1);

-- Upper Deck
INSERT INTO SeatLayouts (LayoutId, BusId, SeatNumber, SeatType, Deck, PositionX, PositionY, IsAvailable)
VALUES 
    ('88888888-8888-8888-8888-888888888819', '77777777-7777-7777-7777-777777777771', 'U1', 1, 1, 0, 0, 1),
    ('88888888-8888-8888-8888-888888888820', '77777777-7777-7777-7777-777777777771', 'U2', 1, 1, 0, 1, 1),
    ('88888888-8888-8888-8888-888888888821', '77777777-7777-7777-7777-777777777771', 'U3', 1, 1, 2, 0, 1),
    ('88888888-8888-8888-8888-888888888822', '77777777-7777-7777-7777-777777777771', 'U4', 1, 1, 2, 1, 1),
    ('88888888-8888-8888-8888-888888888823', '77777777-7777-7777-7777-777777777771', 'U5', 1, 1, 0, 2, 1),
    ('88888888-8888-8888-8888-888888888824', '77777777-7777-7777-7777-777777777771', 'U6', 1, 1, 0, 3, 1),
    ('88888888-8888-8888-8888-888888888825', '77777777-7777-7777-7777-777777777771', 'U7', 1, 1, 2, 2, 1),
    ('88888888-8888-8888-8888-888888888826', '77777777-7777-7777-7777-777777777771', 'U8', 1, 1, 2, 3, 1),
    ('88888888-8888-8888-8888-888888888827', '77777777-7777-7777-7777-777777777771', 'U9', 1, 1, 0, 4, 1),
    ('88888888-8888-8888-8888-888888888828', '77777777-7777-7777-7777-777777777771', 'U10', 1, 1, 0, 5, 1),
    ('88888888-8888-8888-8888-888888888829', '77777777-7777-7777-7777-777777777771', 'U11', 1, 1, 2, 4, 1),
    ('88888888-8888-8888-8888-888888888830', '77777777-7777-7777-7777-777777777771', 'U12', 1, 1, 2, 5, 1),
    ('88888888-8888-8888-8888-888888888831', '77777777-7777-7777-7777-777777777771', 'U13', 1, 1, 0, 6, 1),
    ('88888888-8888-8888-8888-888888888832', '77777777-7777-7777-7777-777777777771', 'U14', 1, 1, 0, 7, 1),
    ('88888888-8888-8888-8888-888888888833', '77777777-7777-7777-7777-777777777771', 'U15', 1, 1, 2, 6, 1),
    ('88888888-8888-8888-8888-888888888834', '77777777-7777-7777-7777-777777777771', 'U16', 1, 1, 2, 7, 1),
    ('88888888-8888-8888-8888-888888888835', '77777777-7777-7777-7777-777777777771', 'U17', 1, 1, 0, 8, 1),
    ('88888888-8888-8888-8888-888888888836', '77777777-7777-7777-7777-777777777771', 'U18', 1, 1, 2, 8, 1);

-- Seat layout for VRL-101 (30 seater sleeper)
INSERT INTO SeatLayouts (LayoutId, BusId, SeatNumber, SeatType, Deck, PositionX, PositionY, IsAvailable)
VALUES 
    ('88888888-8888-8888-8888-888888888841', '77777777-7777-7777-7777-777777777774', 'L1', 0, 0, 0, 0, 1),
    ('88888888-8888-8888-8888-888888888842', '77777777-7777-7777-7777-777777777774', 'L2', 0, 0, 0, 1, 1),
    ('88888888-8888-8888-8888-888888888843', '77777777-7777-7777-7777-777777777774', 'L3', 0, 0, 2, 0, 1),
    ('88888888-8888-8888-8888-888888888844', '77777777-7777-7777-7777-777777777774', 'L4', 0, 0, 2, 1, 1),
    ('88888888-8888-8888-8888-888888888845', '77777777-7777-7777-7777-777777777774', 'L5', 0, 0, 0, 2, 1),
    ('88888888-8888-8888-8888-888888888846', '77777777-7777-7777-7777-777777777774', 'L6', 0, 0, 0, 3, 1),
    ('88888888-8888-8888-8888-888888888847', '77777777-7777-7777-7777-777777777774', 'L7', 0, 0, 2, 2, 1),
    ('88888888-8888-8888-8888-888888888848', '77777777-7777-7777-7777-777777777774', 'L8', 0, 0, 2, 3, 1),
    ('88888888-8888-8888-8888-888888888849', '77777777-7777-7777-7777-777777777774', 'L9', 0, 0, 0, 4, 1),
    ('88888888-8888-8888-8888-888888888850', '77777777-7777-7777-7777-777777777774', 'L10', 0, 0, 0, 5, 1),
    ('88888888-8888-8888-8888-888888888851', '77777777-7777-7777-7777-777777777774', 'L11', 0, 0, 2, 4, 1),
    ('88888888-8888-8888-8888-888888888852', '77777777-7777-7777-7777-777777777774', 'L12', 0, 0, 2, 5, 1),
    ('88888888-8888-8888-8888-888888888853', '77777777-7777-7777-7777-777777777774', 'L13', 0, 0, 0, 6, 1),
    ('88888888-8888-8888-8888-888888888854', '77777777-7777-7777-7777-777777777774', 'L14', 0, 0, 0, 7, 1),
    ('88888888-8888-8888-8888-888888888855', '77777777-7777-7777-7777-777777777774', 'L15', 0, 0, 2, 6, 1),
    ('88888888-8888-8888-8888-888888888856', '77777777-7777-7777-7777-777777777774', 'U1', 1, 1, 0, 0, 1),
    ('88888888-8888-8888-8888-888888888857', '77777777-7777-7777-7777-777777777774', 'U2', 1, 1, 0, 1, 1),
    ('88888888-8888-8888-8888-888888888858', '77777777-7777-7777-7777-777777777774', 'U3', 1, 1, 2, 0, 1),
    ('88888888-8888-8888-8888-888888888859', '77777777-7777-7777-7777-777777777774', 'U4', 1, 1, 2, 1, 1),
    ('88888888-8888-8888-8888-888888888860', '77777777-7777-7777-7777-777777777774', 'U5', 1, 1, 0, 2, 1),
    ('88888888-8888-8888-8888-888888888861', '77777777-7777-7777-7777-777777777774', 'U6', 1, 1, 0, 3, 1),
    ('88888888-8888-8888-8888-888888888862', '77777777-7777-7777-7777-777777777774', 'U7', 1, 1, 2, 2, 1),
    ('88888888-8888-8888-8888-888888888863', '77777777-7777-7777-7777-777777777774', 'U8', 1, 1, 2, 3, 1),
    ('88888888-8888-8888-8888-888888888864', '77777777-7777-7777-7777-777777777774', 'U9', 1, 1, 0, 4, 1),
    ('88888888-8888-8888-8888-888888888865', '77777777-7777-7777-7777-777777777774', 'U10', 1, 1, 0, 5, 1),
    ('88888888-8888-8888-8888-888888888866', '77777777-7777-7777-7777-777777777774', 'U11', 1, 1, 2, 4, 1),
    ('88888888-8888-8888-8888-888888888867', '77777777-7777-7777-7777-777777777774', 'U12', 1, 1, 2, 5, 1),
    ('88888888-8888-8888-8888-888888888868', '77777777-7777-7777-7777-777777777774', 'U13', 1, 1, 0, 6, 1),
    ('88888888-8888-8888-8888-888888888869', '77777777-7777-7777-7777-777777777774', 'U14', 1, 1, 0, 7, 1),
    ('88888888-8888-8888-8888-888888888870', '77777777-7777-7777-7777-777777777774', 'U15', 1, 1, 2, 6, 1);

-- =============================================
-- 7. SCHEDULES TABLE
-- =============================================
INSERT INTO Schedules (ScheduleId, BusId, RouteId, DepartureTime, ArrivalTime, BaseFare, AvailableDatesJson, IsActive)
VALUES 
    -- Bangalore to Chennai schedules
    ('99999999-9999-9999-9999-999999999991', '77777777-7777-7777-7777-777777777771', '55555555-5555-5555-5555-555555555551', '2025-01-01 21:00:00', '2025-01-02 03:00:00', 850.00, NULL, 1),
    ('99999999-9999-9999-9999-999999999992', '77777777-7777-7777-7777-777777777772', '55555555-5555-5555-5555-555555555551', '2025-01-01 22:00:00', '2025-01-02 04:00:00', 650.00, NULL, 1),
    ('99999999-9999-9999-9999-999999999993', '77777777-7777-7777-7777-777777777776', '55555555-5555-5555-5555-555555555551', '2025-01-01 23:00:00', '2025-01-02 05:00:00', 900.00, NULL, 1),
    
    -- Bangalore to Hyderabad schedules
    ('99999999-9999-9999-9999-999999999994', '77777777-7777-7777-7777-777777777774', '55555555-5555-5555-5555-555555555552', '2025-01-01 20:00:00', '2025-01-02 05:00:00', 1200.00, NULL, 1),
    ('99999999-9999-9999-9999-999999999995', '77777777-7777-7777-7777-777777777775', '55555555-5555-5555-5555-555555555552', '2025-01-01 21:30:00', '2025-01-02 06:30:00', 950.00, NULL, 1),
    
    -- Bangalore to Mysore schedules
    ('99999999-9999-9999-9999-999999999996', '77777777-7777-7777-7777-777777777778', '55555555-5555-5555-5555-555555555560', '2025-01-01 06:00:00', '2025-01-01 09:00:00', 350.00, NULL, 1),
    ('99999999-9999-9999-9999-999999999997', '77777777-7777-7777-7777-777777777779', '55555555-5555-5555-5555-555555555560', '2025-01-01 08:00:00', '2025-01-01 11:00:00', 250.00, NULL, 1),
    ('99999999-9999-9999-9999-999999999998', '77777777-7777-7777-7777-777777777780', '55555555-5555-5555-5555-555555555560', '2025-01-01 14:00:00', '2025-01-01 17:00:00', 450.00, NULL, 1),
    
    -- Bangalore to Goa schedules
    ('99999999-9999-9999-9999-999999999999', '77777777-7777-7777-7777-777777777771', '55555555-5555-5555-5555-555555555557', '2025-01-01 19:00:00', '2025-01-02 05:00:00', 1100.00, NULL, 1);

-- =============================================
-- 8. TRIPS TABLE
-- =============================================
-- Generate trips for the next 30 days
DECLARE @TripDate DATE = GETUTCDATE()
DECLARE @EndDate DATE = DATEADD(DAY, 30, GETUTCDATE())
DECLARE @Counter INT = 1

WHILE @TripDate <= @EndDate
BEGIN
    -- Trip for schedule 1 (Bangalore to Chennai - RB-001)
    INSERT INTO Trips (TripId, ScheduleId, TripDate, DepartureDateTime, ArrivalDateTime, CurrentStatus, AvailableSeats, CreatedAt)
    VALUES (NEWID(), '99999999-9999-9999-9999-999999999991', @TripDate, 
            DATEADD(HOUR, 21, CAST(@TripDate AS DATETIME)), 
            DATEADD(HOUR, 27, CAST(@TripDate AS DATETIME)), 
            0, 36, GETUTCDATE());
    
    -- Trip for schedule 2 (Bangalore to Chennai - RB-002)
    INSERT INTO Trips (TripId, ScheduleId, TripDate, DepartureDateTime, ArrivalDateTime, CurrentStatus, AvailableSeats, CreatedAt)
    VALUES (NEWID(), '99999999-9999-9999-9999-999999999992', @TripDate, 
            DATEADD(HOUR, 22, CAST(@TripDate AS DATETIME)), 
            DATEADD(HOUR, 28, CAST(@TripDate AS DATETIME)), 
            0, 40, GETUTCDATE());
    
    -- Trip for schedule 4 (Bangalore to Hyderabad - VRL-101)
    INSERT INTO Trips (TripId, ScheduleId, TripDate, DepartureDateTime, ArrivalDateTime, CurrentStatus, AvailableSeats, CreatedAt)
    VALUES (NEWID(), '99999999-9999-9999-9999-999999999994', @TripDate, 
            DATEADD(HOUR, 20, CAST(@TripDate AS DATETIME)), 
            DATEADD(HOUR, 29, CAST(@TripDate AS DATETIME)), 
            0, 30, GETUTCDATE());
    
    -- Trip for schedule 6 (Bangalore to Mysore - KSRTC-301)
    INSERT INTO Trips (TripId, ScheduleId, TripDate, DepartureDateTime, ArrivalDateTime, CurrentStatus, AvailableSeats, CreatedAt)
    VALUES (NEWID(), '99999999-9999-9999-9999-999999999996', @TripDate, 
            DATEADD(HOUR, 6, CAST(@TripDate AS DATETIME)), 
            DATEADD(HOUR, 9, CAST(@TripDate AS DATETIME)), 
            0, 44, GETUTCDATE());
    
    -- Trip for schedule 7 (Bangalore to Mysore - KSRTC-302)
    INSERT INTO Trips (TripId, ScheduleId, TripDate, DepartureDateTime, ArrivalDateTime, CurrentStatus, AvailableSeats, CreatedAt)
    VALUES (NEWID(), '99999999-9999-9999-9999-999999999997', @TripDate, 
            DATEADD(HOUR, 8, CAST(@TripDate AS DATETIME)), 
            DATEADD(HOUR, 11, CAST(@TripDate AS DATETIME)), 
            0, 54, GETUTCDATE());

    SET @TripDate = DATEADD(DAY, 1, @TripDate)
END

-- Insert specific trips with known IDs for sample bookings
INSERT INTO Trips (TripId, ScheduleId, TripDate, DepartureDateTime, ArrivalDateTime, CurrentStatus, AvailableSeats, CreatedAt)
VALUES 
    ('AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', '99999999-9999-9999-9999-999999999991', DATEADD(DAY, 5, CAST(GETUTCDATE() AS DATE)), 
     DATEADD(HOUR, 21, CAST(DATEADD(DAY, 5, GETUTCDATE()) AS DATETIME)), 
     DATEADD(HOUR, 27, CAST(DATEADD(DAY, 5, GETUTCDATE()) AS DATETIME)), 0, 32, GETUTCDATE()),
    ('AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAB', '99999999-9999-9999-9999-999999999994', DATEADD(DAY, 7, CAST(GETUTCDATE() AS DATE)), 
     DATEADD(HOUR, 20, CAST(DATEADD(DAY, 7, GETUTCDATE()) AS DATETIME)), 
     DATEADD(HOUR, 29, CAST(DATEADD(DAY, 7, GETUTCDATE()) AS DATETIME)), 0, 28, GETUTCDATE());

-- =============================================
-- 9. OFFERS TABLE
-- =============================================
INSERT INTO Offers (OfferId, OfferCode, Description, DiscountType, DiscountValue, MinBookingAmount, MaxDiscount, ValidFrom, ValidTo, UsageLimit, TimesUsed, IsActive)
VALUES 
    ('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBB01', 'WELCOME50', 'Get 50% off on your first booking', 0, 50.00, 500.00, 200.00, DATEADD(DAY, -30, GETUTCDATE()), DATEADD(DAY, 60, GETUTCDATE()), 1000, 150, 1),
    ('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBB02', 'FLAT100', 'Flat Rs.100 off on all bookings', 1, 100.00, 300.00, 100.00, DATEADD(DAY, -15, GETUTCDATE()), DATEADD(DAY, 45, GETUTCDATE()), 500, 75, 1),
    ('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBB03', 'SUMMER25', 'Summer Special - 25% off', 0, 25.00, 400.00, 150.00, DATEADD(DAY, -10, GETUTCDATE()), DATEADD(DAY, 90, GETUTCDATE()), 2000, 320, 1),
    ('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBB04', 'WEEKEND15', 'Weekend special 15% discount', 0, 15.00, 200.00, 100.00, DATEADD(DAY, -5, GETUTCDATE()), DATEADD(DAY, 30, GETUTCDATE()), 800, 200, 1),
    ('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBB05', 'NEWUSER', 'New user special - Rs.150 off', 1, 150.00, 500.00, 150.00, DATEADD(DAY, 0, GETUTCDATE()), DATEADD(DAY, 120, GETUTCDATE()), 5000, 0, 1),
    ('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBB06', 'HOLIDAY30', 'Holiday season 30% off', 0, 30.00, 600.00, 250.00, DATEADD(DAY, 10, GETUTCDATE()), DATEADD(DAY, 40, GETUTCDATE()), 1500, 0, 1),
    ('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBB07', 'EXPIRED20', 'Expired offer - 20% off', 0, 20.00, 300.00, 100.00, DATEADD(DAY, -60, GETUTCDATE()), DATEADD(DAY, -30, GETUTCDATE()), 500, 450, 0),
    ('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBB08', 'LONGTRIP', 'Long distance travel - Rs.200 off', 1, 200.00, 1000.00, 200.00, DATEADD(DAY, -20, GETUTCDATE()), DATEADD(DAY, 100, GETUTCDATE()), 3000, 890, 1);

-- =============================================
-- 10. SAMPLE BOOKINGS
-- =============================================
INSERT INTO Bookings (BookingId, UserId, TripId, BookingReference, TotalSeats, TotalFare, BookingStatus, BookingDate, PassengerDetailsJson)
VALUES 
    ('CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC01', '33333333-3333-3333-3333-333333333331', 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 
     'BK20250101ABC123', 2, 1700.00, 0, DATEADD(DAY, -2, GETUTCDATE()), 
     '[{"Name":"John Doe","Age":34,"Gender":"Male","SeatNumber":"L1"},{"Name":"Jane Doe","Age":32,"Gender":"Female","SeatNumber":"L2"}]'),
    
    ('CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC02', '33333333-3333-3333-3333-333333333332', 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAB', 
     'BK20250102DEF456', 1, 1200.00, 0, DATEADD(DAY, -1, GETUTCDATE()), 
     '[{"Name":"Jane Smith","Age":32,"Gender":"Female","SeatNumber":"L5"}]'),
    
    ('CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC03', '33333333-3333-3333-3333-333333333333', 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 
     'BK20250103GHI789', 3, 2550.00, 0, DATEADD(DAY, -3, GETUTCDATE()), 
     '[{"Name":"Mike Wilson","Age":36,"Gender":"Male","SeatNumber":"U1"},{"Name":"Sarah Wilson","Age":34,"Gender":"Female","SeatNumber":"U2"},{"Name":"Tom Wilson","Age":12,"Gender":"Male","SeatNumber":"U3"}]'),
    
    ('CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC04', '33333333-3333-3333-3333-333333333334', 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAB', 
     'BK20250104JKL012', 1, 1200.00, 1, DATEADD(DAY, -5, GETUTCDATE()), 
     '[{"Name":"Sarah Johnson","Age":29,"Gender":"Female","SeatNumber":"L8"}]'),
    
    ('CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC05', '33333333-3333-3333-3333-333333333335', 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 
     'BK20250105MNO345', 2, 1700.00, 2, DATEADD(DAY, -10, GETUTCDATE()), 
     '[{"Name":"David Brown","Age":37,"Gender":"Male","SeatNumber":"L10"},{"Name":"Emily Brown","Age":35,"Gender":"Female","SeatNumber":"L11"}]');

-- =============================================
-- 11. BOOKED SEATS
-- =============================================
INSERT INTO BookedSeats (BookedSeatId, BookingId, SeatNumber, PassengerName, PassengerAge, PassengerGender, BoardingPointId, DroppingPointId)
VALUES 
    -- Booking 1 seats
    (NEWID(), 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC01', 'L1', 'John Doe', 34, 'Male', '66666666-6666-6666-6666-666666666661', '66666666-6666-6666-6666-666666666667'),
    (NEWID(), 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC01', 'L2', 'Jane Doe', 32, 'Female', '66666666-6666-6666-6666-666666666661', '66666666-6666-6666-6666-666666666667'),
    
    -- Booking 2 seats
    (NEWID(), 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC02', 'L5', 'Jane Smith', 32, 'Female', '66666666-6666-6666-6666-666666666671', '66666666-6666-6666-6666-666666666675'),
    
    -- Booking 3 seats
    (NEWID(), 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC03', 'U1', 'Mike Wilson', 36, 'Male', '66666666-6666-6666-6666-666666666661', '66666666-6666-6666-6666-666666666667'),
    (NEWID(), 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC03', 'U2', 'Sarah Wilson', 34, 'Female', '66666666-6666-6666-6666-666666666661', '66666666-6666-6666-6666-666666666667'),
    (NEWID(), 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC03', 'U3', 'Tom Wilson', 12, 'Male', '66666666-6666-6666-6666-666666666661', '66666666-6666-6666-6666-666666666667'),
    
    -- Booking 4 seats (cancelled)
    (NEWID(), 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC04', 'L8', 'Sarah Johnson', 29, 'Female', '66666666-6666-6666-6666-666666666671', '66666666-6666-6666-6666-666666666675'),
    
    -- Booking 5 seats (completed)
    (NEWID(), 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC05', 'L10', 'David Brown', 37, 'Male', '66666666-6666-6666-6666-666666666661', '66666666-6666-6666-6666-666666666667'),
    (NEWID(), 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC05', 'L11', 'Emily Brown', 35, 'Female', '66666666-6666-6666-6666-666666666661', '66666666-6666-6666-6666-666666666667');

-- =============================================
-- 12. PAYMENTS TABLE
-- =============================================
INSERT INTO Payments (PaymentId, BookingId, Amount, PaymentMethod, PaymentStatus, TransactionId, PaymentDate)
VALUES 
    (NEWID(), 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC01', 1700.00, 0, 1, 'TXN20250101001234ABCD', DATEADD(DAY, -2, GETUTCDATE())),
    (NEWID(), 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC02', 1200.00, 1, 1, 'TXN20250102005678EFGH', DATEADD(DAY, -1, GETUTCDATE())),
    (NEWID(), 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC03', 2550.00, 2, 1, 'TXN20250103009012IJKL', DATEADD(DAY, -3, GETUTCDATE())),
    (NEWID(), 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC04', 1200.00, 3, 3, 'TXN20250104003456MNOP', DATEADD(DAY, -5, GETUTCDATE())),
    (NEWID(), 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC05', 1700.00, 0, 1, 'TXN20250105007890QRST', DATEADD(DAY, -10, GETUTCDATE()));

-- =============================================
-- 13. CANCELLATIONS TABLE
-- =============================================
INSERT INTO Cancellations (CancellationId, BookingId, CancelledById, CancellationReason, RefundAmount, CancellationCharges, CancellationDate, RefundStatus, CreatedAt)
VALUES 
    (NEWID(), 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCC04', '33333333-3333-3333-3333-333333333334', 
     'Change of travel plans', 1080.00, 120.00, DATEADD(DAY, -4, GETUTCDATE()), 1, DATEADD(DAY, -4, GETUTCDATE()));

-- =============================================
-- 14. REVIEWS TABLE
-- =============================================
INSERT INTO Reviews (ReviewId, UserId, TripId, OperatorId, Rating, Comment, CreatedAt)
VALUES 
    (NEWID(), '33333333-3333-3333-3333-333333333335', 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', '44444444-4444-4444-4444-444444444441', 
     5, 'Excellent service! The bus was clean and comfortable. Driver was very professional. Highly recommended!', DATEADD(DAY, -8, GETUTCDATE())),
    
    (NEWID(), '33333333-3333-3333-3333-333333333331', 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', '44444444-4444-4444-4444-444444444441', 
     4, 'Good experience overall. Bus departed on time. WiFi was a bit slow but manageable.', DATEADD(DAY, -5, GETUTCDATE())),
    
    (NEWID(), '33333333-3333-3333-3333-333333333333', 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', '44444444-4444-4444-4444-444444444441', 
     5, 'Best sleeper bus I have traveled in. Very spacious seats and excellent amenities.', DATEADD(DAY, -6, GETUTCDATE())),
    
    (NEWID(), '33333333-3333-3333-3333-333333333332', 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAB', '44444444-4444-4444-4444-444444444442', 
     4, 'VRL maintains good quality. Food was provided at stops. Comfortable journey.', DATEADD(DAY, -3, GETUTCDATE())),
    
    (NEWID(), '33333333-3333-3333-3333-333333333336', 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAB', '44444444-4444-4444-4444-444444444442', 
     3, 'Average experience. Bus was slightly delayed but staff was helpful.', DATEADD(DAY, -2, GETUTCDATE())),
    
    (NEWID(), '33333333-3333-3333-3333-333333333337', 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', '44444444-4444-4444-4444-444444444441', 
     5, 'Amazing journey! Will definitely book again.', DATEADD(DAY, -1, GETUTCDATE()));

-- =============================================
-- 15. NOTIFICATIONS TABLE
-- =============================================
INSERT INTO Notifications (NotificationId, UserId, NotificationType, Subject, Message, IsRead, SentAt, CreatedAt)
VALUES 
    -- Booking confirmations
    (NEWID(), '33333333-3333-3333-3333-333333333331', 0, 'Booking Confirmed - BK20250101ABC123', 
     'Your booking from Bangalore to Chennai on ' + CONVERT(VARCHAR, DATEADD(DAY, 5, GETUTCDATE()), 106) + ' has been confirmed. Have a safe journey!', 
     1, DATEADD(DAY, -2, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE())),
    
    (NEWID(), '33333333-3333-3333-3333-333333333332', 0, 'Booking Confirmed - BK20250102DEF456', 
     'Your booking from Bangalore to Hyderabad on ' + CONVERT(VARCHAR, DATEADD(DAY, 7, GETUTCDATE()), 106) + ' has been confirmed.', 
     1, DATEADD(DAY, -1, GETUTCDATE()), DATEADD(DAY, -1, GETUTCDATE())),
    
    (NEWID(), '33333333-3333-3333-3333-333333333333', 0, 'Booking Confirmed - BK20250103GHI789', 
     'Your booking for 3 passengers from Bangalore to Chennai has been confirmed.', 
     0, DATEADD(DAY, -3, GETUTCDATE()), DATEADD(DAY, -3, GETUTCDATE())),
    
    -- Cancellation notification
    (NEWID(), '33333333-3333-3333-3333-333333333334', 0, 'Booking Cancelled - BK20250104JKL012', 
     'Your booking has been cancelled. Refund of Rs.1080 will be processed within 5-7 business days.', 
     1, DATEADD(DAY, -4, GETUTCDATE()), DATEADD(DAY, -4, GETUTCDATE())),
    
    -- Trip completion notification
    (NEWID(), '33333333-3333-3333-3333-333333333335', 0, 'Trip Completed - BK20250105MNO345', 
     'Thank you for traveling with us! We hope you had a pleasant journey. Please rate your experience.', 
     0, DATEADD(DAY, -8, GETUTCDATE()), DATEADD(DAY, -8, GETUTCDATE())),
    
    -- Promotional notifications
    (NEWID(), '33333333-3333-3333-3333-333333333331', 2, 'Special Offer: 50% OFF!', 
     'Use code WELCOME50 to get 50% off on your next booking. Valid till ' + CONVERT(VARCHAR, DATEADD(DAY, 30, GETUTCDATE()), 106), 
     0, GETUTCDATE(), GETUTCDATE()),
    
    (NEWID(), '33333333-3333-3333-3333-333333333332', 2, 'Weekend Sale Live!', 
     'Book your weekend getaway now and save up to 25% with code WEEKEND15.', 
     0, GETUTCDATE(), GETUTCDATE()),
    
    (NEWID(), '33333333-3333-3333-3333-333333333336', 0, 'Complete Your Profile', 
     'Add your phone number to receive important updates about your bookings.', 
     0, DATEADD(DAY, -1, GETUTCDATE()), DATEADD(DAY, -1, GETUTCDATE())),
    
    (NEWID(), '33333333-3333-3333-3333-333333333337', 0, 'Verify Your Email', 
     'Please verify your email address to unlock all features.', 
     1, DATEADD(DAY, -7, GETUTCDATE()), DATEADD(DAY, -7, GETUTCDATE())),
    
    (NEWID(), '33333333-3333-3333-3333-333333333338', 0, 'Email Verification Required', 
     'Your account is not verified. Please check your email for verification link.', 
     0, DATEADD(DAY, -1, GETUTCDATE()), DATEADD(DAY, -1, GETUTCDATE()));

-- =============================================
-- SUMMARY
-- =============================================
PRINT '=========================================='
PRINT 'Database Seed Completed Successfully!'
PRINT '=========================================='
PRINT ''
PRINT 'Data Inserted:'
PRINT '  - Users: 17 (2 Admin, 5 Operators, 10 Passengers)'
PRINT '  - Bus Operators: 5'
PRINT '  - Routes: 10'
PRINT '  - Stops: 17'
PRINT '  - Buses: 10'
PRINT '  - Seat Layouts: 66'
PRINT '  - Schedules: 9'
PRINT '  - Trips: 150+ (30 days of trips)'
PRINT '  - Offers: 8'
PRINT '  - Bookings: 5'
PRINT '  - Booked Seats: 9'
PRINT '  - Payments: 5'
PRINT '  - Cancellations: 1'
PRINT '  - Reviews: 6'
PRINT '  - Notifications: 10'
PRINT ''
PRINT 'Test Credentials:'
PRINT '  Admin: admin@busbooking.com / Password@123'
PRINT '  Operator: operator1@redbus.com / Password@123'
PRINT '  Passenger: john.doe@gmail.com / Password@123'
PRINT '=========================================='
GO
