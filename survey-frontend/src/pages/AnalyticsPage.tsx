import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { getSurveyAnalytics, getSurveys } from '../api/surveys';
import { BarChart3, Users, Clock, TrendingUp, Activity } from 'lucide-react';
import type { SurveyAnalyticsDto, SurveyDto } from '../types';

export const AnalyticsPage: React.FC = () => {
  const { surveyId } = useParams<{ surveyId?: string }>();
  const [selectedSurveyId, setSelectedSurveyId] = useState<string>(surveyId || '');
  const [surveys, setSurveys] = useState<SurveyDto[]>([]);
  const [analytics, setAnalytics] = useState<SurveyAnalyticsDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [surveysLoading, setSurveysLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchSurveys = async () => {
      try {
        const response = await getSurveys({ pageNumber: 1, pageSize: 100 });
        setSurveys(response.items);
        
        // If no surveyId in URL and surveys exist, select the first one
        if (!surveyId && response.items.length > 0) {
          setSelectedSurveyId(response.items[0].id);
        }
      } catch (err) {
        setError('Failed to load surveys');
        console.error('Error fetching surveys:', err);
      } finally {
        setSurveysLoading(false);
      }
    };

    fetchSurveys();
  }, [surveyId]);

  useEffect(() => {
    if (selectedSurveyId) {
      fetchAnalytics(selectedSurveyId);
    }
  }, [selectedSurveyId]);

  const fetchAnalytics = async (id: string) => {
    setLoading(true);
    setError(null);
    
    try {
      const data = await getSurveyAnalytics(id);
      setAnalytics(data);
    } catch (err) {
      setError('Failed to load analytics data');
      console.error('Error fetching analytics:', err);
      setAnalytics(null);
    } finally {
      setLoading(false);
    }
  };

  const formatDuration = (minutes: number) => {
    if (minutes < 1) return '< 1 min';
    if (minutes < 60) return `${Math.round(minutes)} min`;
    
    const hours = Math.floor(minutes / 60);
    const remainingMinutes = Math.round(minutes % 60);
    
    if (remainingMinutes === 0) return `${hours}h`;
    return `${hours}h ${remainingMinutes}m`;
  };

  const formatPercentage = (value: number) => {
    return `${(value * 100).toFixed(1)}%`;
  };

  if (surveysLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Analytics</h1>
      </div>

      {surveys.length === 0 ? (
        <div className="bg-white shadow rounded-lg p-6">
          <div className="text-center">
            <BarChart3 className="mx-auto h-12 w-12 text-gray-400" />
            <h3 className="mt-2 text-sm font-medium text-gray-900">No surveys available</h3>
            <p className="mt-1 text-sm text-gray-500">
              Create some surveys to view analytics.
            </p>
          </div>
        </div>
      ) : (
        <>
          {/* Survey Selector */}
          <div className="bg-white shadow rounded-lg p-4">
            <label htmlFor="survey-select" className="block text-sm font-medium text-gray-700 mb-2">
              Select Survey
            </label>
            <select
              id="survey-select"
              value={selectedSurveyId}
              onChange={(e) => setSelectedSurveyId(e.target.value)}
              className="block w-full max-w-md border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
            >
              <option value="">Choose a survey...</option>
              {surveys.map((survey) => (
                <option key={survey.id} value={survey.id}>
                  {survey.title}
                </option>
              ))}
            </select>
          </div>

          {/* Analytics Content */}
          {selectedSurveyId && (
            <>
              {loading ? (
                <div className="flex items-center justify-center h-64">
                  <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                </div>
              ) : error ? (
                <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
                  {error}
                </div>
              ) : analytics ? (
                <div className="space-y-6">
                  {/* Survey Info */}
                  <div className="bg-white shadow rounded-lg p-6">
                    <h2 className="text-xl font-semibold text-gray-900 mb-2">
                      {analytics.surveyTitle}
                    </h2>
                    <p className="text-gray-600">Survey Analytics Overview</p>
                  </div>

                  {/* Key Metrics */}
                  <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
                    <div className="bg-white overflow-hidden shadow rounded-lg">
                      <div className="p-5">
                        <div className="flex items-center">
                          <div className="flex-shrink-0">
                            <div className="bg-blue-500 rounded-md p-3">
                              <BarChart3 className="h-6 w-6 text-white" />
                            </div>
                          </div>
                          <div className="ml-5 w-0 flex-1">
                            <dl>
                              <dt className="text-sm font-medium text-gray-500 truncate">
                                Total Questions
                              </dt>
                              <dd className="text-lg font-medium text-gray-900">
                                {analytics.totalQuestions}
                              </dd>
                            </dl>
                          </div>
                        </div>
                      </div>
                    </div>

                    <div className="bg-white overflow-hidden shadow rounded-lg">
                      <div className="p-5">
                        <div className="flex items-center">
                          <div className="flex-shrink-0">
                            <div className="bg-green-500 rounded-md p-3">
                              <Users className="h-6 w-6 text-white" />
                            </div>
                          </div>
                          <div className="ml-5 w-0 flex-1">
                            <dl>
                              <dt className="text-sm font-medium text-gray-500 truncate">
                                Total Responses
                              </dt>
                              <dd className="text-lg font-medium text-gray-900">
                                {analytics.totalResponses}
                              </dd>
                            </dl>
                          </div>
                        </div>
                      </div>
                    </div>

                    <div className="bg-white overflow-hidden shadow rounded-lg">
                      <div className="p-5">
                        <div className="flex items-center">
                          <div className="flex-shrink-0">
                            <div className="bg-yellow-500 rounded-md p-3">
                              <Activity className="h-6 w-6 text-white" />
                            </div>
                          </div>
                          <div className="ml-5 w-0 flex-1">
                            <dl>
                              <dt className="text-sm font-medium text-gray-500 truncate">
                                Tokens Generated
                              </dt>
                              <dd className="text-lg font-medium text-gray-900">
                                {analytics.tokensGenerated}
                              </dd>
                            </dl>
                          </div>
                        </div>
                      </div>
                    </div>

                    <div className="bg-white overflow-hidden shadow rounded-lg">
                      <div className="p-5">
                        <div className="flex items-center">
                          <div className="flex-shrink-0">
                            <div className="bg-purple-500 rounded-md p-3">
                              <TrendingUp className="h-6 w-6 text-white" />
                            </div>
                          </div>
                          <div className="ml-5 w-0 flex-1">
                            <dl>
                              <dt className="text-sm font-medium text-gray-500 truncate">
                                Response Rate
                              </dt>
                              <dd className="text-lg font-medium text-gray-900">
                                {formatPercentage(analytics.responseRate)}
                              </dd>
                            </dl>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* Detailed Analytics */}
                  <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
                    {/* Completion Stats */}
                    <div className="bg-white shadow rounded-lg p-6">
                      <h3 className="text-lg font-medium text-gray-900 mb-4">
                        Completion Statistics
                      </h3>
                      <div className="space-y-4">
                        <div className="flex items-center justify-between">
                          <span className="text-sm text-gray-600">Average Completion Time</span>
                          <div className="flex items-center">
                            <Clock className="h-4 w-4 text-gray-400 mr-1" />
                            <span className="text-sm font-medium text-gray-900">
                              {formatDuration(analytics.averageCompletionTime)}
                            </span>
                          </div>
                        </div>
                        
                        <div className="flex items-center justify-between">
                          <span className="text-sm text-gray-600">Completion Rate</span>
                          <span className="text-sm font-medium text-gray-900">
                            {formatPercentage(analytics.responseRate)}
                          </span>
                        </div>

                        <div>
                          <div className="flex items-center justify-between mb-1">
                            <span className="text-sm text-gray-600">Progress</span>
                            <span className="text-sm font-medium text-gray-900">
                              {analytics.totalResponses} / {analytics.tokensGenerated}
                            </span>
                          </div>
                          <div className="w-full bg-gray-200 rounded-full h-2">
                            <div
                              className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                              style={{
                                width: `${Math.min(analytics.responseRate * 100, 100)}%`,
                              }}
                            ></div>
                          </div>
                        </div>
                      </div>
                    </div>

                    {/* Response Distribution */}
                    <div className="bg-white shadow rounded-lg p-6">
                      <h3 className="text-lg font-medium text-gray-900 mb-4">
                        Response Overview
                      </h3>
                      <div className="space-y-4">
                        <div className="flex items-center justify-between p-3 bg-green-50 rounded-lg">
                          <div className="flex items-center">
                            <div className="w-3 h-3 bg-green-500 rounded-full mr-3"></div>
                            <span className="text-sm font-medium text-green-800">Completed</span>
                          </div>
                          <span className="text-sm font-bold text-green-900">
                            {analytics.totalResponses}
                          </span>
                        </div>
                        
                        <div className="flex items-center justify-between p-3 bg-yellow-50 rounded-lg">
                          <div className="flex items-center">
                            <div className="w-3 h-3 bg-yellow-500 rounded-full mr-3"></div>
                            <span className="text-sm font-medium text-yellow-800">Pending</span>
                          </div>
                          <span className="text-sm font-bold text-yellow-900">
                            {analytics.tokensGenerated - analytics.totalResponses}
                          </span>
                        </div>

                        <div className="flex items-center justify-between p-3 bg-blue-50 rounded-lg">
                          <div className="flex items-center">
                            <div className="w-3 h-3 bg-blue-500 rounded-full mr-3"></div>
                            <span className="text-sm font-medium text-blue-800">Total Sent</span>
                          </div>
                          <span className="text-sm font-bold text-blue-900">
                            {analytics.tokensGenerated}
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              ) : (
                <div className="bg-white shadow rounded-lg p-6">
                  <div className="text-center">
                    <BarChart3 className="mx-auto h-12 w-12 text-gray-400" />
                    <h3 className="mt-2 text-sm font-medium text-gray-900">No analytics data</h3>
                    <p className="mt-1 text-sm text-gray-500">
                      Analytics data will appear once responses are received.
                    </p>
                  </div>
                </div>
              )}
            </>
          )}
        </>
      )}
    </div>
  );
};