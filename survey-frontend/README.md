# Survey Frontend Application

A modern React-based survey management system for administrators, built with TypeScript and Vite.

## Features

- **Admin Authentication**: Secure login system for administrators
- **Survey Management**: Create, view, and manage surveys
- **Anonymous Responses**: No user registration required for survey participants
- **Email Invitations**: Send survey invitations via email
- **Analytics Dashboard**: View survey analytics and response statistics
- **Responsive Design**: Mobile-friendly interface
- **Real-time Updates**: Live data updates for survey statistics

## Tech Stack

- **Frontend**: React 19, TypeScript, Vite
- **Routing**: React Router DOM
- **Forms**: React Hook Form with Zod validation
- **HTTP Client**: Axios
- **Icons**: Lucide React
- **Date Handling**: date-fns
- **Styling**: Custom CSS with utility classes (Tailwind-like)

## Getting Started

### Prerequisites

- Node.js (version 18 or higher)
- pnpm (recommended) or npm
- Backend API server running on `http://localhost:5299/api`

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd survey-frontend
```

2. Install dependencies:
```bash
pnpm install
```

3. Start the development server:
```bash
pnpm dev
```

The application will be available at `http://localhost:5173`

### Build for Production

```bash
pnpm build
```

## API Configuration

The application expects the backend API to be running on `http://localhost:5299/api`. You can modify this in `src/api/api.ts` if needed.

## Application Structure

```
src/
├── api/           # API service functions
├── components/    # Reusable React components
├── contexts/      # React contexts (Authentication)
├── pages/         # Page components
├── types/         # TypeScript type definitions
├── App.tsx        # Main application component
├── main.tsx       # Application entry point
└── index.css      # Global styles
```

## Pages and Features

### Admin Authentication
- **Login Page**: Secure admin login with email and password
- **Protected Routes**: All admin pages require authentication
- **Token Management**: JWT token storage and refresh

### Dashboard
- **Overview**: Quick stats and recent surveys
- **Navigation**: Easy access to all features
- **Responsive Layout**: Works on desktop and mobile

### Survey Management
- **Survey List**: View all surveys with filtering and search
- **Create Survey**: Build surveys with multiple question types
- **Survey Details**: View individual survey information
- **Question Types**: Support for text, multiple choice, single choice, rating, and yes/no questions

### Invitations
- **Email Invitations**: Send survey invitations to multiple recipients
- **Bulk Operations**: Add multiple email addresses
- **Delivery Status**: Track successful and failed invitations
- **Custom Messages**: Include personal messages with invitations

### Analytics
- **Survey Analytics**: Detailed statistics for each survey
- **Response Tracking**: Monitor response rates and completion times
- **Visual Data**: Charts and graphs for better data visualization
- **Real-time Updates**: Live data updates as responses come in

### Settings
- **Admin Management**: Create and manage administrator accounts
- **System Information**: View application and system details

## Authentication Flow

1. Admin enters email and password on login page
2. Credentials are sent to `/api/Admins/login` endpoint
3. On success, JWT token is stored in localStorage
4. Token is included in all subsequent API requests
5. Protected routes check for valid authentication
6. Logout clears token and redirects to login

## Survey Creation Flow

1. Admin creates survey with title, description, and questions
2. Multiple question types supported with options
3. Survey is saved as draft initially
4. Admin can publish survey to make it available
5. Published surveys can receive invitations

## Invitation Flow

1. Admin selects published survey
2. Enters recipient email addresses
3. Optional custom message
4. System sends invitations with unique survey links
5. Recipients receive anonymous survey links
6. No user registration required for participants

## API Endpoints Used

- `POST /api/Admins/login` - Admin authentication
- `GET /api/Admins` - List administrators
- `POST /api/Admins` - Create new administrator
- `GET /api/Surveys` - List surveys
- `POST /api/Surveys` - Create new survey
- `GET /api/Surveys/{id}` - Get survey details
- `GET /api/Surveys/{id}/analytics` - Get survey analytics
- `POST /api/Surveys/invitations` - Send survey invitations
- `GET /api/Surveys/published` - List published surveys

## Security Features

- JWT-based authentication
- Protected routes
- Token expiration handling
- Secure API communication
- Input validation and sanitization

## Usage Instructions

1. **Start the Backend API**: Ensure your backend API is running on `http://localhost:5299/api`
2. **Start the Frontend**: Run `pnpm dev` to start the development server
3. **Login**: Use admin credentials to access the dashboard
4. **Create Surveys**: Navigate to Surveys → Create Survey to build your first survey
5. **Send Invitations**: Once published, use the Invitations page to send survey links
6. **Monitor Analytics**: View response data and statistics in the Analytics section

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License.
