# Buckeye Lending Demo

A reference architecture that helps you understand:

- **Using states and props with React** – Learn how to manage component state and pass data between components using props
- **Using controllers to build APIs with ASP.NET** – Learn how to create RESTful endpoints using ASP.NET controllers

## Project Structure

```text
buckeye-lending-demo/
├── frontend/          # React + TypeScript + Vite
│   └── src/
│       └── components/
└── backend/           # ASP.NET API
│   └── Buckeye.Lending.Api/
│       └── Controllers/
```

## Getting Started

### Frontend

```bash
cd frontend
npm install
npm run dev
```

### Backend

```bash
cd backend/Buckeye.Lending.Api
dotnet run
```

Use the above commands to start both the frontend and backend servers. The frontend will be available at `http://localhost:5173` and the backend API will be available at `http://localhost:5000/api`.

Inside the `backend/Buckeye.Lending.Api/Controllers` folder, you'll find the file `Buckeye.Lending.Api.http` which contains test requests for the API. When the API is running, you can use these requests to test the endpoints and see how they work. Alternatively, you can use Swagger UI at `http://localhost:5000/swagger` to send requests to the API.

## Learning Objectives

### React States and Props

- Understanding `useState` for managing component state
- Passing data from parent to child components via props
- Lifting state up to share data between sibling components

### ASP.NET Controllers

- Creating controller classes to handle HTTP requests
- Defining routes and action methods
- Returning JSON responses from API endpoints

> [!NOTE]
> Use this demo as a starting point to build your own applications, and to help you with the Milestone 2 assignment. Feel free to modify and expand upon the code as needed!

For M2, your architecture diagram should include your planned API endpoints. This table maps your M2 features to the endpoints you'd build.

You won't code these APIs for M2 — just design them. Include them in your architecture diagram showing which React components call which endpoints. That shows a clear understanding of full-stack design.

**For your M2 architecture diagram, plan your endpoints:**

| Feature             | Endpoints to Design                                                         |
| ------------------- | --------------------------------------------------------------------------- |
| Loan Applications   | `GET /api/applications`, `POST /api/applications`                           |
| Application Details | `GET /api/applications/{id}`, `PUT /api/applications/{id}`                  |
| Loan Products       | `GET /api/loanproducts`, `GET /api/loanproducts/{id}`                       |
| Borrower Management | `GET /api/borrowers/{id}`, `POST /api/borrowers`                            |
| Document Upload     | `POST /api/applications/{id}/documents`, `GET /api/documents/{id}`          |
| Underwriting        | `GET /api/applications/{id}/decision`, `POST /api/applications/{id}/submit` |

```text
ApplicationsController:  GET, GET/{id}, POST, PUT/{id}, DELETE/{id}
LoanProductsController:  GET, GET/{id}
BorrowersController:     GET/{id}, POST, PUT/{id}
DocumentsController:     GET/{id}, POST (upload)
```

**Think about:**

- What data does each React component need? → That's a GET endpoint
- What forms do users fill out? → That's a POST endpoint
- What can users modify? → That's a PUT endpoint
- What can users remove? → That's a DELETE endpoint
- How does a user get all products under $20? → That's a GET endpoint with query parameters

From the frontend perspective: every piece of data your React components display needs to come from an API. Every button that saves something needs to call an API. Map those out.
