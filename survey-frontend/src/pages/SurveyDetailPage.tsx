import React, { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { getSurveyById, publishSurvey } from '../api/surveys';
import { ArrowLeft, Calendar, User, BarChart3, Mail, Send, CheckCircle } from 'lucide-react';
import type { SurveyDto } from '../types';
import { format } from 'date-fns';

export const SurveyDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [survey, setSurvey] = useState<SurveyDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isPublishing, setIsPublishing] = useState(false);
  const [publishSuccess, setPublishSuccess] = useState(false);

  useEffect(() => {
    if (id) {
      fetchSurvey(id);
    }
  }, [id]);

  const fetchSurvey = async (surveyId: string) => {
    try {
      setLoading(true);
      const data = await getSurveyById(surveyId);
      setSurvey(data);
    } catch (err) {
      setError('Failed to load survey details');
      console.error('Error fetching survey:', err);
    } finally {
      setLoading(false);
    }
  };

  const handlePublishSurvey = async () => {
    if (!survey || survey.status === 'Published') return;
    
    setIsPublishing(true);
    setError(null);
    
    try {
      const updatedSurvey = await publishSurvey(survey.id);
      setSurvey(updatedSurvey);
      setPublishSuccess(true);
      
      // Hide success message after 3 seconds
      setTimeout(() => {
        setPublishSuccess(false);
      }, 3000);
    } catch (err) {
      setError('Failed to publish survey. Please try again.');
      console.error('Error publishing survey:', err);
    } finally {
      setIsPublishing(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (error || !survey) {
    return (
      <div className="max-w-4xl mx-auto space-y-6">
        <div className="flex items-center space-x-4">
          <button
            onClick={() => navigate('/admin/surveys')}
            className="inline-flex items-center text-sm text-gray-500 hover:text-gray-700"
          >
            <ArrowLeft className="mr-1 h-4 w-4" />
            Back to Surveys
          </button>
        </div>
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
          {error || 'Survey not found'}
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div className="flex items-center space-x-4">
        <button
          onClick={() => navigate('/admin/surveys')}
          className="inline-flex items-center text-sm text-gray-500 hover:text-gray-700"
        >
          <ArrowLeft className="mr-1 h-4 w-4" />
          Back to Surveys
        </button>
      </div>

      {publishSuccess && (
        <div className="bg-green-50 border border-green-200 rounded-md p-4 mb-6">
          <div className="flex items-center">
            <CheckCircle className="h-5 w-5 text-green-500 mr-2" />
            <div className="text-sm font-medium text-green-800">
              Survey published successfully! You can now send invitations.
            </div>
          </div>
        </div>
      )}

      <div className="bg-white shadow rounded-lg overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-200">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-2xl font-bold text-gray-900">{survey.title}</h1>
              <div className="flex items-center mt-2 text-sm text-gray-500">
                <User className="h-4 w-4 mr-1" />
                <span>Created by {survey.createdByName}</span>
                <span className="mx-2">•</span>
                <Calendar className="h-4 w-4 mr-1" />
                <span>{format(new Date(survey.createdAt), 'MMM d, yyyy')}</span>
              </div>
            </div>
            <span
              className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium ${
                survey.status === 'Published'
                  ? 'bg-green-100 text-green-800'
                  : survey.status === 'Draft'
                  ? 'bg-yellow-100 text-yellow-800'
                  : 'bg-gray-100 text-gray-800'
              }`}
            >
              {survey.status}
            </span>
          </div>
        </div>

        <div className="p-6">
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-3">
            <div className="bg-blue-50 p-4 rounded-lg">
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <div className="w-8 h-8 bg-blue-500 rounded-md flex items-center justify-center">
                    <span className="text-white font-medium text-sm">{survey.questionCount}</span>
                  </div>
                </div>
                <div className="ml-3">
                  <p className="text-sm font-medium text-blue-800">Questions</p>
                  <p className="text-xs text-blue-600">Total questions in survey</p>
                </div>
              </div>
            </div>

            <div className="bg-green-50 p-4 rounded-lg">
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <div className="w-8 h-8 bg-green-500 rounded-md flex items-center justify-center">
                    <span className="text-white font-medium text-sm">{survey.responseCount}</span>
                  </div>
                </div>
                <div className="ml-3">
                  <p className="text-sm font-medium text-green-800">Responses</p>
                  <p className="text-xs text-green-600">Completed responses</p>
                </div>
              </div>
            </div>

            <div className="bg-purple-50 p-4 rounded-lg">
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <div className="w-8 h-8 bg-purple-500 rounded-md flex items-center justify-center">
                    <Calendar className="h-4 w-4 text-white" />
                  </div>
                </div>
                <div className="ml-3">
                  <p className="text-sm font-medium text-purple-800">Last Updated</p>
                  <p className="text-xs text-purple-600">
                    {format(new Date(survey.updatedAt), 'MMM d, yyyy')}
                  </p>
                </div>
              </div>
            </div>
          </div>

          <div className="mt-8 flex flex-col sm:flex-row gap-4">
            <Link
              to={`/admin/analytics/${survey.id}`}
              className="inline-flex items-center justify-center px-6 py-3 border border-transparent text-sm font-semibold rounded-lg text-white bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700 transform hover:scale-105 transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 shadow-lg hover:shadow-xl"
            >
              <BarChart3 className="mr-2 h-4 w-4" />
              <span>View Analytics</span>
            </Link>
            
            {survey.status === 'Draft' && (
              <button
                onClick={handlePublishSurvey}
                disabled={isPublishing}
                className="inline-flex items-center justify-center px-6 py-3 border border-transparent text-sm font-semibold rounded-lg text-white bg-gradient-to-r from-green-500 to-emerald-600 hover:from-green-600 hover:to-emerald-700 transform hover:scale-105 transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500 shadow-lg hover:shadow-xl disabled:opacity-60 disabled:cursor-not-allowed disabled:transform-none disabled:hover:scale-100"
              >
                {isPublishing ? (
                  <>
                    <div className="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent mr-2"></div>
                    <span>Publishing...</span>
                  </>
                ) : (
                  <>
                    <Send className="mr-2 h-4 w-4" />
                    <span>Publish Survey</span>
                  </>
                )}
              </button>
            )}
            
            {survey.status === 'Published' && (
              <>
                <Link
                  to={`/admin/invitations?surveyId=${survey.id}`}
                  className="inline-flex items-center justify-center px-6 py-3 border border-transparent text-sm font-semibold rounded-lg text-white bg-gradient-to-r from-purple-500 to-pink-600 hover:from-purple-600 hover:to-pink-700 transform hover:scale-105 transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-purple-500 shadow-lg hover:shadow-xl"
                >
                  <Mail className="mr-2 h-4 w-4" />
                  <span>Send Invitations</span>
                </Link>
                <Link
                  to="/admin/invitations"
                  className="inline-flex items-center justify-center px-6 py-3 border border-blue-200 text-sm font-semibold rounded-lg text-blue-700 bg-gradient-to-r from-blue-50 to-indigo-50 hover:from-blue-100 hover:to-indigo-100 transform hover:scale-105 transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 shadow-md hover:shadow-lg"
                >
                  <Mail className="mr-2 h-4 w-4" />
                  <span>Manage Invitations</span>
                </Link>
              </>
            )}
          </div>
        </div>
      </div>

      {/* Survey Summary */}
      <div className="bg-white shadow rounded-lg">
        <div className="px-6 py-4 border-b border-gray-200">
          <h2 className="text-lg font-medium text-gray-900">Survey Summary</h2>
        </div>
        <div className="p-6">
          <dl className="grid grid-cols-1 gap-x-4 gap-y-6 sm:grid-cols-2">
            <div>
              <dt className="text-sm font-medium text-gray-500">Survey ID</dt>
              <dd className="mt-1 text-sm text-gray-900 font-mono">{survey.id}</dd>
            </div>
            <div>
              <dt className="text-sm font-medium text-gray-500">Status</dt>
              <dd className="mt-1">
                <span
                  className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                    survey.status === 'Published'
                      ? 'bg-green-100 text-green-800'
                      : survey.status === 'Draft'
                      ? 'bg-yellow-100 text-yellow-800'
                      : 'bg-gray-100 text-gray-800'
                  }`}
                >
                  {survey.status}
                </span>
              </dd>
            </div>
            <div>
              <dt className="text-sm font-medium text-gray-500">Created At</dt>
              <dd className="mt-1 text-sm text-gray-900">
                {format(new Date(survey.createdAt), 'EEEE, MMMM d, yyyy \'at\' h:mm a')}
              </dd>
            </div>
            <div>
              <dt className="text-sm font-medium text-gray-500">Last Updated</dt>
              <dd className="mt-1 text-sm text-gray-900">
                {format(new Date(survey.updatedAt), 'EEEE, MMMM d, yyyy \'at\' h:mm a')}
              </dd>
            </div>
            <div>
              <dt className="text-sm font-medium text-gray-500">Created By</dt>
              <dd className="mt-1 text-sm text-gray-900">{survey.createdByName}</dd>
            </div>
          </dl>
        </div>
      </div>
    </div>
  );
};