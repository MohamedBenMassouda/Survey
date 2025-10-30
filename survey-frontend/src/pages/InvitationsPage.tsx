import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { useSearchParams } from 'react-router-dom';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { getPublishedSurveys, sendInvitations } from '../api/surveys';
import { Mail, Plus, Trash2, Send, CheckCircle, XCircle } from 'lucide-react';
import type { SurveyDto, InvitationResponse } from '../types';

const invitationSchema = z.object({
  surveyId: z.string().min(1, 'Please select a survey'),
  recipientEmails: z.array(z.string().email('Invalid email address')).min(1, 'At least one email is required'),
  customMessage: z.string().optional(),
});

type InvitationFormData = z.infer<typeof invitationSchema>;

export const InvitationsPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const preSelectedSurveyId = searchParams.get('surveyId');
  
  const [surveys, setSurveys] = useState<SurveyDto[]>([]);
  const [emails, setEmails] = useState<string[]>(['']);
  const [isLoading, setIsLoading] = useState(false);
  const [surveysLoading, setSurveysLoading] = useState(true);
  const [result, setResult] = useState<InvitationResponse | null>(null);
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<InvitationFormData>({
    resolver: zodResolver(invitationSchema),
    defaultValues: {
      surveyId: preSelectedSurveyId || '',
      recipientEmails: [],
      customMessage: '',
    },
  });

  useEffect(() => {
    const fetchSurveys = async () => {
      try {
        const response = await getPublishedSurveys({ pageNumber: 1, pageSize: 100 });
        console.log('Published surveys response:', response);
        console.log('Survey items:', response?.items);
        console.log('Survey items length:', response?.items?.length);
        
        // Handle different response formats
        let surveysData: SurveyDto[] = [];
        if (response && response.items && Array.isArray(response.items)) {
          // If response is paginated
          surveysData = response.items;
        } else if (Array.isArray(response)) {
          // If response is directly an array
          surveysData = response as any;
        } else {
          console.warn('Unexpected response format:', response);
          surveysData = [];
        }
        
        setSurveys(surveysData);
        
        // Set pre-selected survey if it exists and is in the list
        if (preSelectedSurveyId && surveysData.some(s => s.id === preSelectedSurveyId)) {
          setValue('surveyId', preSelectedSurveyId);
        }
      } catch (err) {
        setError('Failed to load surveys');
        console.error('Error fetching surveys:', err);
      } finally {
        setSurveysLoading(false);
      }
    };

    fetchSurveys();
  }, []);

  useEffect(() => {
    const validEmails = emails.filter(email => email.trim() !== '');
    setValue('recipientEmails', validEmails);
  }, [emails, setValue]);

  const addEmailField = () => {
    setEmails([...emails, '']);
  };

  const removeEmailField = (index: number) => {
    if (emails.length > 1) {
      const newEmails = emails.filter((_, i) => i !== index);
      setEmails(newEmails);
    }
  };

  const updateEmail = (index: number, value: string) => {
    const newEmails = [...emails];
    newEmails[index] = value;
    setEmails(newEmails);
  };

  const onSubmit = async (data: InvitationFormData) => {
    setIsLoading(true);
    setError(null);
    setResult(null);

    try {
      const validEmails = emails.filter(email => email.trim() !== '' && /\S+@\S+\.\S+/.test(email.trim()));
      
      if (validEmails.length === 0) {
        setError('Please enter at least one valid email address.');
        setIsLoading(false);
        return;
      }
      
      if (!data.surveyId) {
        setError('Please select a survey.');
        setIsLoading(false);
        return;
      }

      const invitationData = {
        surveyId: data.surveyId,
        recipientEmails: validEmails,
        customMessage: data.customMessage,
      };

      console.log('Sending invitations with data:', invitationData);
      const response = await sendInvitations(invitationData);
      console.log('Invitation response:', response);
      setResult(response);
    } catch (err: any) {
      console.error('Error sending invitations:', err);
      console.error('Error response:', err.response?.data);
      setError(err.response?.data?.detail || err.response?.data?.title || 'Failed to send invitations. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const resetForm = () => {
    setEmails(['']);
    setResult(null);
    setError(null);
    setValue('surveyId', '');
    setValue('customMessage', '');
  };

  if (surveysLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Send Invitations</h1>
      </div>

      {surveys.length === 0 ? (
        <div className="bg-white shadow rounded-lg p-6">
          <div className="text-center">
            <Mail className="mx-auto h-12 w-12 text-gray-400" />
            <h3 className="mt-2 text-sm font-medium text-gray-900">No published surveys</h3>
            <p className="mt-1 text-sm text-gray-500">
              You need to have published surveys before you can send invitations.
            </p>
          </div>
        </div>
      ) : (
        <div className="bg-white shadow rounded-lg">
          <div className="px-6 py-4 border-b border-gray-200">
            <h2 className="text-lg font-medium text-gray-900">Create Invitation</h2>
            <p className="text-sm text-gray-500">Send survey invitations to participants via email</p>
            {preSelectedSurveyId && (
              <div className="mt-2 bg-blue-50 border border-blue-200 rounded-md p-3">
                <p className="text-sm text-blue-800">
                  Survey pre-selected from detail page
                </p>
              </div>
            )}
          </div>

          <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-6">
            {/* Survey Selection */}
            <div>
              <label htmlFor="surveyId" className="block text-sm font-medium text-gray-700">
                Select Survey
              </label>
              <select
                {...register('surveyId')}
                id="surveyId"
                className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
              >
                <option value="">Choose a survey...</option>
                {surveys.map((survey) => (
                  <option key={survey.id} value={survey.id}>
                    {survey.title} ({survey.questionCount} questions)
                  </option>
                ))}
              </select>
              {errors.surveyId && (
                <p className="mt-1 text-sm text-red-600">{errors.surveyId.message}</p>
              )}
            </div>

            {/* Email Recipients */}
            <div>
              <div className="flex items-center justify-between mb-2">
                <label className="block text-sm font-medium text-gray-700">
                  Recipient Emails
                </label>
                <button
                  type="button"
                  onClick={addEmailField}
                  className="inline-flex items-center text-xs text-blue-600 hover:text-blue-800"
                >
                  <Plus className="mr-1 h-3 w-3" />
                  Add Email
                </button>
              </div>
              
              <div className="space-y-2">
                {emails.map((email, index) => (
                  <div key={index} className="flex items-center space-x-2">
                    <input
                      type="email"
                      value={email}
                      onChange={(e) => updateEmail(index, e.target.value)}
                      className="flex-1 border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                      placeholder="Enter email address"
                    />
                    {emails.length > 1 && (
                      <button
                        type="button"
                        onClick={() => removeEmailField(index)}
                        className="text-red-600 hover:text-red-800"
                      >
                        <Trash2 className="h-4 w-4" />
                      </button>
                    )}
                  </div>
                ))}
              </div>
              
              {errors.recipientEmails && (
                <p className="mt-1 text-sm text-red-600">{errors.recipientEmails.message}</p>
              )}
            </div>

            {/* Custom Message */}
            <div>
              <label htmlFor="customMessage" className="block text-sm font-medium text-gray-700">
                Custom Message (Optional)
              </label>
              <textarea
                {...register('customMessage')}
                id="customMessage"
                rows={4}
                className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                placeholder="Hello! You've been invited to participate in our survey. Your feedback is valuable to us and will help improve our services. The survey is completely anonymous and should take just a few minutes to complete. Thank you for your time!"
              />
              <p className="mt-1 text-xs text-gray-500">
                This message will be included in the email invitation along with the survey link.
              </p>
            </div>

            {error && (
              <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
                {error}
              </div>
            )}

            {/* Results */}
            {result && (
              <div className="bg-gray-50 p-4 rounded-lg">
                <h3 className="text-lg font-medium text-gray-900 mb-4">Invitation Results</h3>
                
                <div className="grid grid-cols-1 gap-4 sm:grid-cols-3 mb-4">
                  <div className="bg-blue-100 p-3 rounded-lg">
                    <div className="text-sm font-medium text-blue-800">Total Invitations</div>
                    <div className="text-2xl font-bold text-blue-900">{result.totalInvitations}</div>
                  </div>
                  <div className="bg-green-100 p-3 rounded-lg">
                    <div className="text-sm font-medium text-green-800">Successful</div>
                    <div className="text-2xl font-bold text-green-900">{result.successfulInvitations?.length || 0}</div>
                  </div>
                  <div className="bg-red-100 p-3 rounded-lg">
                    <div className="text-sm font-medium text-red-800">Failed</div>
                    <div className="text-2xl font-bold text-red-900">{result.failedInvitations?.length || 0}</div>
                  </div>
                </div>

                {result.successfulInvitations && result.successfulInvitations.length > 0 && (
                  <div className="mb-4">
                    <h4 className="text-sm font-medium text-green-800 mb-2 flex items-center">
                      <CheckCircle className="mr-1 h-4 w-4" />
                      Successfully sent to:
                    </h4>
                    <ul className="text-sm text-green-700 space-y-1">
                      {result.successfulInvitations.map((email, index) => (
                        <li key={index} className="flex items-center">
                          <span className="w-2 h-2 bg-green-500 rounded-full mr-2"></span>
                          {email}
                        </li>
                      ))}
                    </ul>
                  </div>
                )}

                {result.failedInvitations && result.failedInvitations.length > 0 && (
                  <div>
                    <h4 className="text-sm font-medium text-red-800 mb-2 flex items-center">
                      <XCircle className="mr-1 h-4 w-4" />
                      Failed to send to:
                    </h4>
                    <ul className="text-sm text-red-700 space-y-1">
                      {result.failedInvitations.map((failure, index) => (
                        <li key={index} className="flex items-start">
                          <span className="w-2 h-2 bg-red-500 rounded-full mr-2 mt-1.5"></span>
                          <div>
                            <div>{failure.email}</div>
                            <div className="text-xs text-red-600">{failure.errorMessage}</div>
                          </div>
                        </li>
                      ))}
                    </ul>
                  </div>
                )}

                <div className="mt-4 pt-4 border-t border-gray-200">
                  <button
                    type="button"
                    onClick={resetForm}
                    className="text-sm text-blue-600 hover:text-blue-800"
                  >
                    Send More Invitations
                  </button>
                </div>
              </div>
            )}

            <div className="flex justify-end space-x-3">
              <button
                type="button"
                onClick={resetForm}
                className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              >
                Reset
              </button>
              <button
                type="submit"
                disabled={isLoading}
                className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isLoading ? (
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                ) : (
                  <Send className="mr-2 h-4 w-4" />
                )}
                Send Invitations
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
};