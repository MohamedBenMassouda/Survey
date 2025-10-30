import React, { useState, useEffect } from 'react';
import { useParams, useSearchParams } from 'react-router-dom';
import { CheckCircle, AlertCircle } from 'lucide-react';
import { getPublicSurvey, submitSurveyResponse, type PublicSurvey, type SubmitAnswerRequest } from '../api/public';

export const PublicSurveyPage: React.FC = () => {
  const { surveyId } = useParams<{ surveyId: string }>();
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');
  const [survey, setSurvey] = useState<PublicSurvey | null>(null);
  const [responses, setResponses] = useState<Record<string, any>>({});
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    // Fetch survey data using both surveyId and token
    if (surveyId && token) {
      fetchPublicSurvey(surveyId, token);
    } else {
      setError('Invalid survey link. Missing survey ID or token.');
      setLoading(false);
    }
  }, [surveyId, token]);

  const fetchPublicSurvey = async (surveyId: string, token: string) => {
    try {
      setLoading(true);
      console.log('Fetching survey with ID:', surveyId, 'and token:', token);
      const surveyData = await getPublicSurvey(`${surveyId}?token=${token}`);
      console.log('Survey data received:', surveyData);
      console.log('Questions in survey:', surveyData.questions);
      setSurvey(surveyData);
    } catch (err: any) {
      if (err.response?.status === 404) {
        setError('Survey not found. The invitation link may be invalid or expired.');
      } else if (err.response?.status === 410) {
        setError('This survey has expired and is no longer accepting responses.');
      } else {
        setError('Failed to load survey. Please try again later.');
      }
      console.error('Error fetching public survey:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleResponseChange = (questionId: string, value: any) => {
    setResponses(prev => ({
      ...prev,
      [questionId]: value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!survey) return;

    // Validate required questions
    const requiredQuestions = survey.questions.filter((q: any) => q.isRequired);
    const missingResponses = requiredQuestions.filter((q: any) => !responses[q.id]);
    
    if (missingResponses.length > 0) {
      setError('Please answer all required questions.');
      return;
    }

    setSubmitting(true);
    setError(null);

    try {
      if (!surveyId || !token) {
        setError('Invalid survey link.');
        return;
      }
      
      // Convert responses to API format
      const answers: SubmitAnswerRequest[] = Object.entries(responses).map(([questionId, response]) => {
        const question = survey.questions.find(q => q.id === questionId);
        
        if (!question) {
          return {
            questionId,
            answerText: String(response),
          };
        }
        
        // Handle different question types
        switch (question.questionType) {
          case 'Text':
            return {
              questionId,
              answerText: String(response),
            };
          
          case 'MultipleChoice':
          case 'YesNo':
          case 'Rating':
          case 'Scale':
            // For single selection questions, put the answer in answerText
            return {
              questionId,
              answerText: String(response),
            };
          
          case 'Checkbox':
            // For multiple selection, use selectedOptionIds if they're option IDs
            if (Array.isArray(response)) {
              return {
                questionId,
                selectedOptionIds: response.map(String),
              };
            } else {
              return {
                questionId,
                answerText: String(response),
              };
            }
          
          case 'Dropdown':
            // Check if response is an option ID or text
            if (question.options?.some(opt => opt.id === response)) {
              return {
                questionId,
                selectedOptionIds: [String(response)],
              };
            } else {
              return {
                questionId,
                answerText: String(response),
              };
            }
          
          default:
            return {
              questionId,
              answerText: String(response),
            };
        }
      });
      
      await submitSurveyResponse(token, answers);
      setSubmitted(true);
    } catch (err: any) {
      if (err.response?.status === 409) {
        setError('You have already submitted a response to this survey.');
      } else if (err.response?.status === 410) {
        setError('This survey has expired and is no longer accepting responses.');
      } else {
        setError('Failed to submit survey. Please try again.');
      }
      console.error('Error submitting survey:', err);
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (error && !survey) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="max-w-md w-full">
          <div className="bg-white shadow rounded-lg p-6">
            <div className="flex items-center">
              <AlertCircle className="h-8 w-8 text-red-500 mr-3" />
              <div>
                <h3 className="text-lg font-medium text-gray-900">Survey Not Available</h3>
                <p className="text-sm text-gray-600 mt-1">{error}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (submitted) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="max-w-md w-full">
          <div className="bg-white shadow rounded-lg p-6 text-center">
            <CheckCircle className="h-16 w-16 text-green-500 mx-auto mb-4" />
            <h3 className="text-xl font-medium text-gray-900 mb-2">Thank You!</h3>
            <p className="text-gray-600">
              Your responses have been submitted successfully. Your participation is anonymous and helps us improve our services.
            </p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-2xl mx-auto">
        <div className="bg-white shadow rounded-lg">
          <div className="px-6 py-8">
            <div className="text-center mb-8">
              <h1 className="text-2xl font-bold text-gray-900">{survey?.title}</h1>
              <p className="text-gray-600 mt-2">{survey?.description}</p>
              <div className="mt-4 text-sm text-gray-500">
                This survey is completely anonymous. No personal information is collected.
              </div>
            </div>

            <form onSubmit={handleSubmit} className="space-y-8">              
              {survey?.questions.sort((a, b) => a.displayOrder - b.displayOrder).map((question: any) => (
                <div key={question.id} className="space-y-3">
                  <label className="block text-sm font-medium text-gray-900">
                    {question.displayOrder}. {question.questionText}
                    {question.isRequired && <span className="text-red-500 ml-1">*</span>}
                  </label>

                  {(question.questionType === 'Text') && (
                    <textarea
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      rows={4}
                      placeholder="Enter your response..."
                      value={responses[question.id] || ''}
                      onChange={(e) => handleResponseChange(question.id, e.target.value)}
                    />
                  )}

                  {(question.questionType === 'MultipleChoice') && question.options && (
                    <div className="space-y-2">
                      {question.options.map((option: any) => (
                        <label key={option.id} className="flex items-center">
                          <input
                            type="radio"
                            name={question.id}
                            value={option.id}
                            className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
                            checked={responses[question.id] === option.id}
                            onChange={() => handleResponseChange(question.id, option.id)}
                          />
                          <span className="ml-2 text-sm text-gray-700">{option.optionText}</span>
                        </label>
                      ))}
                    </div>
                  )}

                  {(question.questionType === 'Checkbox') && question.options && (
                    <div className="space-y-2">
                      {question.options.map((option: any) => (
                        <label key={option.id} className="flex items-center">
                          <input
                            type="checkbox"
                            name={question.id}
                            value={option.id}
                            className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
                            checked={responses[question.id]?.includes(option.id) || false}
                            onChange={(e) => {
                              const current = responses[question.id] || [];
                              const updated = e.target.checked
                                ? [...current, option.id]
                                : current.filter((id: string) => id !== option.id);
                              handleResponseChange(question.id, updated);
                            }}
                          />
                          <span className="ml-2 text-sm text-gray-700">{option.optionText}</span>
                        </label>
                      ))}
                    </div>
                  )}

                  {(question.questionType === 'MultipleChoice') && !question.options && (
                    <div className="bg-yellow-50 border border-yellow-200 rounded-md p-3">
                      <p className="text-sm text-yellow-800 mb-2">
                        This multiple choice question has no options defined. Please enter your response:
                      </p>
                      <input
                        type="text"
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                        placeholder="Enter your response..."
                        value={responses[question.id] || ''}
                        onChange={(e) => handleResponseChange(question.id, e.target.value)}
                      />
                    </div>
                  )}

                  {(question.questionType === 'YesNo') && (
                    <div className="space-y-2">
                      {['Yes', 'No'].map((option) => (
                        <label key={option} className="flex items-center">
                          <input
                            type="radio"
                            name={question.id}
                            value={option.toLowerCase()}
                            className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
                            checked={responses[question.id] === option.toLowerCase()}
                            onChange={() => handleResponseChange(question.id, option.toLowerCase())}
                          />
                          <span className="ml-2 text-sm text-gray-700">{option}</span>
                        </label>
                      ))}
                    </div>
                  )}

                  {(question.questionType === 'Rating') && (
                    <div className="flex space-x-2">
                      {[1, 2, 3, 4, 5].map((rating) => (
                        <button
                          key={rating}
                          type="button"
                          className={`w-10 h-10 rounded-full border-2 text-sm font-medium transition-colors ${
                            responses[question.id] === rating
                              ? 'bg-blue-600 border-blue-600 text-white'
                              : 'border-gray-300 text-gray-700 hover:border-blue-300 hover:bg-blue-50'
                          }`}
                          onClick={() => handleResponseChange(question.id, rating)}
                        >
                          {rating}
                        </button>
                      ))}
                    </div>
                  )}

                  {(question.questionType === 'Scale') && (
                    <div className="space-y-3">
                      <div className="flex justify-between text-sm text-gray-500">
                        <span>1 (Poor)</span>
                        <span>5 (Excellent)</span>
                      </div>
                      <div className="flex space-x-3">
                        {[1, 2, 3, 4, 5].map((scale) => (
                          <label key={scale} className="flex flex-col items-center space-y-1">
                            <input
                              type="radio"
                              name={question.id}
                              value={scale}
                              className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
                              checked={responses[question.id] === scale}
                              onChange={() => handleResponseChange(question.id, scale)}
                            />
                            <span className="text-sm font-medium text-gray-700">{scale}</span>
                          </label>
                        ))}
                      </div>
                    </div>
                  )}

                  {/* Handle Dropdown question type */}
                  {(question.questionType === 'Dropdown') && question.options && (
                    <select
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      value={responses[question.id] || ''}
                      onChange={(e) => handleResponseChange(question.id, e.target.value)}
                    >
                      <option value="">Select an option...</option>
                      {question.options.map((option: any) => (
                        <option key={option.id} value={option.id}>
                          {option.optionText}
                        </option>
                      ))}
                    </select>
                  )}

                  {/* Fallback for unknown question types */}
                  {!['Text', 'MultipleChoice', 'Checkbox', 'YesNo', 'Rating', 'Scale', 'Dropdown'].includes(question.questionType) && (
                    <div className="bg-yellow-50 border border-yellow-200 rounded-md p-3">
                      <p className="text-sm text-yellow-800 mb-2">
                        Question type: {question.questionType} (using text input as fallback)
                      </p>
                      <textarea
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                        rows={2}
                        placeholder="Enter your response..."
                        value={responses[question.id] || ''}
                        onChange={(e) => handleResponseChange(question.id, e.target.value)}
                      />
                    </div>
                  )}
                </div>
              ))}

              {error && (
                <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
                  {error}
                </div>
              )}

              <div className="pt-6">
                <button
                  type="submit"
                  disabled={submitting}
                  className="w-full flex justify-center py-3 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {submitting ? (
                    <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-white"></div>
                  ) : (
                    'Submit Survey'
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>

        <div className="mt-6 text-center text-xs text-gray-500">
          This survey is completely anonymous. Your responses cannot be traced back to you.
        </div>
      </div>
    </div>
  );
};