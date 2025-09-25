# Project Overview

## What is Student Performance Tracker?

The Student Performance Tracker is an ASP.NET Core MVC web application designed to manage academic performance tracking in educational institutions. The system provides a comprehensive platform for managing courses, classes, student enrollments, and academic grades.

## Purpose & Goals

The application aims to:
- **Streamline Academic Management** - Centralize course and class management
- **Track Student Performance** - Monitor grades and academic progress
- **Support Multiple User Roles** - Admin, Teacher, and Student interfaces
- **Ensure Data Security** - Secure authentication and authorization
- **Provide Scalable Architecture** - Clean, maintainable codebase

## Key Stakeholders

### Students
- Enroll in classes using join codes
- View their grades and academic progress
- Manage their profile information

### Teachers
- Create and manage their classes
- Input and update student grades
- Generate class reports and analytics

### Administrators
- Manage courses and curriculum
- Oversee user accounts and permissions
- Monitor system-wide performance

## Core Features

### User Management
- Secure registration and authentication
- Role-based access control (Admin, Teacher, Student)
- Profile management with photo uploads
- Account approval workflow

### Academic Management
- Course catalog management
- Class scheduling and room assignment
- Student enrollment via join codes
- Grade tracking (midterm and final)

### System Features
- Email notifications and password reset
- Audit trails for data changes
- Responsive web interface
- Database-driven configuration

## Business Rules

### User Approval
- **Students** are automatically approved upon registration
- **Teachers** require admin approval before accessing the system
- **Admins** are created through direct database operations

### Class Management
- Each class belongs to one course and has one teacher
- Classes have unique join codes for student enrollment
- Join codes expire and can be regenerated

### Grading System
- Supports midterm and final grades
- Grades are stored with decimal precision
- Teachers can add remarks for additional context
- Grade changes are tracked with timestamps

## Success Metrics

The project success will be measured by:
- **User Adoption** - Number of active students and teachers
- **Data Accuracy** - Reliable grade tracking and reporting
- **System Performance** - Fast response times and uptime
- **User Satisfaction** - Ease of use and feature completeness

## Future Enhancements

Potential future features include:
- Mobile application support
- Advanced analytics and reporting
- Integration with external systems (LMS, SIS)
- Automated grade calculations
- Parent/guardian access portals

---

*This overview provides the foundation for understanding the project's purpose and scope. For technical details, see the other documentation files.*