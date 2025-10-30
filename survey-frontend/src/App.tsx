
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { DashboardLayout } from './components/DashboardLayout';
import { LoginPage } from './pages/LoginPage';
import { DashboardPage } from './pages/DashboardPage';
import { SurveysPage } from './pages/SurveysPage';
import { CreateSurveyPage } from './pages/CreateSurveyPage';
import { SurveyDetailPage } from './pages/SurveyDetailPage';
import { InvitationsPage } from './pages/InvitationsPage';
import { AnalyticsPage } from './pages/AnalyticsPage';
import { SettingsPage } from './pages/SettingsPage';
import { PublicSurveyPage } from './pages/PublicSurveyPage';
import './App.css';

function App() {
  return (
    <AuthProvider>
      <Router>
        <div className="App">
          <Routes>
            {/* Public routes (no authentication required) */}
            <Route path="/surveys/:surveyId" element={<PublicSurveyPage />} />
            
            {/* Admin authentication */}
            <Route path="/admin/login" element={<LoginPage />} />
            
            {/* Protected admin routes */}
            <Route
              path="/admin"
              element={
                <ProtectedRoute>
                  <DashboardLayout />
                </ProtectedRoute>
              }
            >
              <Route index element={<Navigate to="/admin/dashboard" replace />} />
              <Route path="dashboard" element={<DashboardPage />} />
              <Route path="surveys" element={<SurveysPage />} />
              <Route path="surveys/create" element={<CreateSurveyPage />} />
              <Route path="surveys/:id" element={<SurveyDetailPage />} />
              <Route path="invitations" element={<InvitationsPage />} />
              <Route path="analytics" element={<AnalyticsPage />} />
              <Route path="analytics/:surveyId" element={<AnalyticsPage />} />
              <Route path="settings" element={<SettingsPage />} />
            </Route>
            
            {/* Root redirects */}
            <Route path="/" element={<Navigate to="/admin/login" replace />} />
            <Route path="/login" element={<Navigate to="/admin/login" replace />} />
            <Route path="*" element={<Navigate to="/admin/login" replace />} />
          </Routes>
        </div>
      </Router>
    </AuthProvider>
  );
}

export default App;
