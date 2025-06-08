# Task Management System

## Overview

This project is a comprehensive Task Management System designed to track and manage assignments. It features a clean, modular architecture that separates core business logic from the user interface, making it both scalable and maintainable. The system offers two ways to interact with it: a user-friendly console application and a powerful RESTful Web API for programmatic access.

---

## Features

* **Full CRUD Functionality**: Create, read, update, and delete assignments.
* **Detailed Assignment Properties**: Each assignment includes a title, description, completion status, due date, notes, and a priority level (Low, Medium, or High).
* **Dual Interfaces**:
    * A full-featured **Console Application** for direct, menu-driven user interaction.
    * A RESTful **Web API** built with ASP.NET Core for programmatic access and integration with other services.
* **Robust Testing Suite**: The solution includes a dedicated test project using xUnit and Moq for comprehensive unit and regression testing.
* **Dependency Injection**: Both the console and Web API projects use dependency injection to manage services and promote loose coupling.
* **Comprehensive Logging**: An application-wide logging interface (`IAppLogger`) is implemented to provide detailed information, warnings, and error messages across all layers of the application.

---

## Project Structure

The solution is organized into four distinct projects:

* **`AssignmentManagement.Core`**: The heart of the application. This class library contains the domain models (`Assignment`), core services (`AssignmentService`), and all public interfaces (`IAssignmentService`, `IAppLogger`, etc.).
* **`AssignmentManagement.UI`**: A .NET Console Application that provides a command-line interface (CLI) for users to interact with the system.
* **`AssignmentManagement.WebAPI`**: An ASP.NET Core project that exposes the system's functionality through a RESTful API. It includes controllers and Data Transfer Objects (DTOs) for handling HTTP requests.
* **`AssignmentManagement.Tests`**: An xUnit test project containing unit tests for the Core, UI, and service layers, ensuring code reliability and correctness.

---

## Setup and Usage

### Prerequisites

* .NET 8 SDK (or later)
* Visual Studio 2022

### Installation

1.  Clone this repository to your local machine:
    ```bash
    git clone <your-repository-url>
    ```
2.  Open the `AssignmentManagement.sln` solution file in Visual Studio 2022.

### Running the Console Application

1.  In the Solution Explorer, right-click the **`AssignmentManagement.UI`** project and select **"Set as Startup Project"**.
2.  Press `F5` or the "Start" button to build and run the console application.

### Running the Web API

1.  In the Solution Explorer, right-click the **`AssignmentManagement.WebAPI`** project and select **"Set as Startup Project"**.
2.  Press `F5` or the "Start" button to launch the API.
3.  A browser window will open with the Swagger UI, providing a full list of available API endpoints and allowing you to test them directly.

### Running Tests

1.  Open the **Test Explorer** in Visual Studio (`Test > Test Explorer`).
2.  Click the **"Run All Tests"** button in the Test Explorer to execute all unit tests and verify the system's functionality.

---

## API Endpoints

The Web API provides the following primary endpoints for interacting with assignments:

* `GET /api/assignment`: Retrieves a list of all assignments.
* `GET /api/assignment/{title}`: Retrieves a single assignment by its title.
* `POST /api/assignment`: Creates a new assignment. The request body should contain a JSON object with `title`, `description`, `priority`, `dueDate`, and `notes`.
* `DELETE /api/assignment/{title}`: Deletes an assignment by its title.

---

## Maintenance Plan

This plan outlines the strategy for the ongoing maintenance, code quality, and future-proofing of the Task Management System.

### 1. Code Quality and Reviews
- **Scheduled Code Reviews**: Conduct quarterly code reviews with a focus on ensuring new code adheres to SOLID principles, maintains low complexity, and utilizes the established interfaces and dependency injection patterns.
- **Complexity Monitoring**: Continue to use Visual Studio's Code Metrics to analyze the project. Any new method exceeding a cyclomatic complexity of 10 should be flagged for immediate refactoring.
- **Style Consistency**: Enforce a consistent C# coding style across all projects to improve readability and maintainability.

### 2. Documentation
- **XML Comments**: All new public classes, methods, and properties added to the `AssignmentManagement.Core` library must be documented with descriptive XML comments to support IntelliSense and API documentation.
- **README Updates**: The root `README.md` file, particularly the API Endpoints section, must be updated whenever endpoints are added, changed, or removed.
- **Inline Comments**: Use inline comments to clarify the purpose of any complex or non-obvious blocks of code.

### 3. Testing and Coverage
- **Expand Test Coverage**: Continue to expand the unit test suite with a goal of achieving and maintaining at least 80% code coverage for the `AssignmentManagement.Core` and `AssignmentManagement.WebAPI` projects.
- **Regression Testing**: Every bug fix must be accompanied by a new unit test that specifically reproduces the bug. This test must fail before the fix is applied and pass after, preventing future regressions.
- **New Feature Testing**: All new features must have corresponding unit tests before they are considered complete and merged into the main branch.

### 4. Bug and Issue Management
- **Issue Tracking**: All bugs, feature requests, and enhancements must be tracked as issues in the GitHub repository.
- **Resolution Timeframe**: Acknowledge new issues within 3 business days. Aim to resolve all critical bugs within two weeks of their report date.
- **Clear Reporting**: Encourage bug reports that include clear steps to reproduce, a description of the expected outcome, and the actual outcome.

### 5. Dependency Management
- **Regular Audits**: Perform a review of all third-party NuGet packages every six months to check for updates, security vulnerabilities, and deprecated libraries.
- **Automated Scanning**: Enable a tool like GitHub's Dependabot to automatically scan for vulnerable dependencies and create pull requests for updates.

## Contributing

Contributions are welcome! Please fork the repository and submit pull requests for any enhancements or bug fixes. Ensure that any new code is accompanied by corresponding unit tests.


