import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { getSurveys } from '../api/surveys';
import { useNotifications } from '../contexts/NotificationContext';
import { memeQuotesApi } from '../api/memes';
import { FileText, Users, BarChart3, Plus, TrendingUp, Smile } from 'lucide-react';
import type { SurveyDto } from '../types';

export const DashboardPage: React.FC = () => {
  const [surveys, setSurveys] = useState<SurveyDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { addNotification } = useNotifications();

  useEffect(() => {
    const fetchSurveys = async () => {
      try {
        const response = await getSurveys({ pageNumber: 1, pageSize: 5 });
        setSurveys(response.items);
      } catch (err) {
        setError('Failed to load surveys');
        console.error('Error fetching surveys:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchSurveys();
  }, []);

  const handleTestMemeQuote = async () => {
    try {
      const quote = await memeQuotesApi.getRandomMemeQuote();
      addNotification({
        type: 'meme',
        title: '🎯 Test Meme Quote',
        message: `"${quote.content}" - ${quote.author}`,
        duration: 8000,
      });
    } catch (error) {
      addNotification({
        type: 'error',
        title: 'Error',
        message: 'Failed to fetch meme quote',
        duration: 5000,
      });
    }
  };

  const stats = [
    {
      name: 'Total Surveys',
      value: surveys.length,
      icon: FileText,
      color: 'bg-blue-500',
    },
    {
      name: 'Total Responses',
      value: surveys.reduce((sum, survey) => sum + survey.responseCount, 0),
      icon: Users,
      color: 'bg-green-500',
    },
    {
      name: 'Active Surveys',
      value: surveys.filter(survey => survey.status === 'Published').length,
      icon: TrendingUp,
      color: 'bg-yellow-500',
    },
    {
      name: 'Draft Surveys',
      value: surveys.filter(survey => survey.status === 'Draft').length,
      icon: BarChart3,
      color: 'bg-purple-500',
    },
  ];

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
        <div className="flex items-center space-x-3">
          <button
            onClick={handleTestMemeQuote}
            className="inline-flex items-center px-3 py-2 border border-purple-300 text-sm font-medium rounded-md text-purple-700 bg-purple-50 hover:bg-purple-100 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-purple-500"
          >
            <Smile className="mr-2 h-4 w-4" />
            Test Meme Quote
          </button>
          <Link
            to="/admin/surveys/create"
            className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
          >
            <Plus className="mr-2 h-4 w-4" />
            Create Survey
          </Link>
        </div>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat) => {
          const Icon = stat.icon;
          return (
            <div key={stat.name} className="bg-white overflow-hidden shadow rounded-lg">
              <div className="p-5">
                <div className="flex items-center">
                  <div className="flex-shrink-0">
                    <div className={`${stat.color} rounded-md p-3`}>
                      <Icon className="h-6 w-6 text-white" />
                    </div>
                  </div>
                  <div className="ml-5 w-0 flex-1">
                    <dl>
                      <dt className="text-sm font-medium text-gray-500 truncate">
                        {stat.name}
                      </dt>
                      <dd className="text-lg font-medium text-gray-900">
                        {stat.value}
                      </dd>
                    </dl>
                  </div>
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* Recent Surveys */}
      <div className="bg-white shadow rounded-lg">
        <div className="px-4 py-5 sm:p-6">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg leading-6 font-medium text-gray-900">
              Recent Surveys
            </h3>
            <Link
              to="/admin/surveys"
              className="text-sm text-blue-600 hover:text-blue-500"
            >
              View all
            </Link>
          </div>
          
          {error ? (
            <div className="text-red-600 text-sm">{error}</div>
          ) : surveys.length === 0 ? (
            <div className="text-center py-8">
              <FileText className="mx-auto h-12 w-12 text-gray-400" />
              <h3 className="mt-2 text-sm font-medium text-gray-900">No surveys</h3>
              <p className="mt-1 text-sm text-gray-500">
                Get started by creating a new survey.
              </p>
              <div className="mt-6">
                <Link
                  to="/admin/surveys/create"
                  className="inline-flex items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                >
                  <Plus className="mr-2 h-4 w-4" />
                  Create Survey
                </Link>
              </div>
            </div>
          ) : (
            <div className="overflow-hidden">
              <ul className="divide-y divide-gray-200">
                {surveys.map((survey) => (
                  <li key={survey.id} className="py-4">
                    <div className="flex items-center justify-between">
                      <div className="flex-1">
                        <h4 className="text-sm font-medium text-gray-900">
                          {survey.title}
                        </h4>
                        <div className="flex items-center mt-1 text-sm text-gray-500">
                          <span>Created by {survey.createdByName}</span>
                          <span className="mx-2">•</span>
                          <span>{survey.questionCount} questions</span>
                          <span className="mx-2">•</span>
                          <span>{survey.responseCount} responses</span>
                        </div>
                      </div>
                      <div className="flex items-center space-x-2">
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
                        <Link
                          to={`/admin/surveys/${survey.id}`}
                          className="text-blue-600 hover:text-blue-500 text-sm font-medium"
                        >
                          View
                        </Link>
                      </div>
                    </div>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};