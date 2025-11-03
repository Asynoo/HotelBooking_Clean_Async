# language: en
Feature: Hotel Room Booking Creation
As a hotel customer
I want to book a room for specific dates
So that I can secure accommodation for my stay

    Background:
        Given today's date is 2024-01-01
        And there is a fully occupied period from 2024-01-10 to 2024-01-20

    Scenario: Successful booking before occupied period
        When I request to book a room from 2024-01-05 to 2024-01-07
        Then the booking should be created successfully

    Scenario: Successful booking after occupied period
        When I request to book a room from 2024-01-21 to 2024-01-25
        Then the booking should be created successfully

    Scenario: Successful single day booking
        When I request to book a room from 2024-01-08 to 2024-01-08
        Then the booking should be created successfully

    Scenario: Booking with start date in the past
        When I request to book a room from 2023-12-30 to 2024-01-05
        Then the booking should be rejected with error "The start date cannot be in the past or later than the end date"

    Scenario: Booking with start date after end date
        When I request to book a room from 2024-01-15 to 2024-01-10
        Then the booking should be rejected with error "The start date cannot be in the past or later than the end date"

    Scenario: Booking overlapping start of occupied period
        When I request to book a room from 2024-01-08 to 2024-01-12
        Then the booking should be rejected due to overlap

    Scenario: Booking overlapping end of occupied period
        When I request to book a room from 2024-01-18 to 2024-01-22
        Then the booking should be rejected due to overlap

    Scenario: Booking completely within occupied period
        When I request to book a room from 2024-01-12 to 2024-01-15
        Then the booking should be rejected due to overlap

    Scenario: Booking spanning entire occupied period
        When I request to book a room from 2024-01-05 to 2024-01-25
        Then the booking should be rejected due to overlap

    Scenario: Booking ending on first day of occupied period
        When I request to book a room from 2024-01-07 to 2024-01-10
        Then the booking should be rejected due to overlap

    Scenario: Booking starting on last day of occupied period
        When I request to book a room from 2024-01-20 to 2024-01-22
        Then the booking should be created successfully

    Scenario: Decision table - start before today should reject with error
        When I request to book a room from 2024-01-01 to 2024-01-02
        Then the booking should be rejected with error "The start date cannot be in the past or later than the end date"

    Scenario: Decision table - SD before O, ED first day of O should reject due to overlap
        When I request to book a room from 2024-01-09 to 2024-01-10
        Then the booking should be rejected due to overlap

    Scenario: Decision table - SD before O, ED last day of O should reject due to overlap
        When I request to book a room from 2024-01-09 to 2024-01-20
        Then the booking should be rejected due to overlap

    Scenario: Decision table - SD in O, ED in O should reject due to overlap
        When I request to book a room from 2024-01-10 to 2024-01-10
        Then the booking should be rejected due to overlap

    Scenario: Decision table - SD on last day of O, ED after O should create successfully
        When I request to book a room from 2024-01-20 to 2024-01-21
        Then the booking should be created successfully

    Scenario: Decision table - SD after O, ED after O should create successfully
        When I request to book a room from 2024-01-21 to 2024-01-22
        Then the booking should be created successfully

    Scenario: View available rooms during fully occupied period
        When I check room availability from 2024-01-12 to 2024-01-15
        Then no rooms should be available

    Scenario: View available rooms before occupied period
        When I check room availability from 2024-01-05 to 2024-01-07
        Then rooms should be available

    Scenario: View available rooms after occupied period
        When I check room availability from 2024-01-21 to 2024-01-25
        Then rooms should be available