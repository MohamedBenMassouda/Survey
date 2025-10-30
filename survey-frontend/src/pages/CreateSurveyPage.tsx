import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { createSurvey } from '../api/surveys';
import { Plus, Trash2, ArrowLeft, Save } from 'lucide-react';
import { QuestionType } from '../types';

const optionSchema = z.object({
  optionText: z.string().min(1, 'Option text is required'),
  displayOrder: z.number().optional(),
});

const questionSchema = z.object({
  questionText: z.string().min(1, 'Question text is required'),
  questionType: z.string().min(1, 'Question type is required'),
  isRequired: z.boolean(),
  displayOrder: z.number().optional(),
  options: z.array(optionSchema).optional(),
});

const surveySchema = z.object({
  title: z.string().min(1, 'Survey title is required'),
  description: z.string().min(1, 'Survey description is required'),
  startDate: z.string().optional(),
  endDate: z.string().optional(),
  questions: z.array(questionSchema).min(1, 'At least one question is required'),
});

type SurveyFormData = z.infer<typeof surveySchema>;

const questionTypes = [
  { value: QuestionType.TEXT, label: 'Text' },
  { value: QuestionType.MULTIPLE_CHOICE, label: 'Multiple Choice' },
  { value: QuestionType.CHECKBOX, label: 'Checkbox (Multiple Selection)' },
  { value: QuestionType.RATING, label: 'Rating' },
  { value: QuestionType.YES_NO, label: 'Yes/No' },
  { value: QuestionType.DROPDOWN, label: 'Dropdown' },
  { value: QuestionType.SCALE, label: 'Scale' },
];

export const CreateSurveyPage: React.FC = () => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [publishOnCreate, setPublishOnCreate] = useState(false);
  const navigate = useNavigate();

  const {
    register,
    control,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<SurveyFormData>({
    resolver: zodResolver(surveySchema),
    defaultValues: {
      title: '',
      description: '',
      questions: [
        {
          questionText: '',
          questionType: QuestionType.TEXT,
          isRequired: false,
          options: [],
        },
      ],
    },
  });

  const { fields: questions, append: appendQuestion, remove: removeQuestion } = useFieldArray({
    control,
    name: 'questions',
  });

  const onSubmit = async (data: SurveyFormData) => {
    setIsLoading(true);
    setError(null);

    try {
      // Process the data to match API expectations
      const surveyData = {
        ...data,
        // Convert dates to UTC ISO strings
        startDate: data.startDate ? new Date(data.startDate).toISOString() : undefined,
        endDate: data.endDate ? new Date(data.endDate).toISOString() : undefined,
        status: publishOnCreate ? 'Published' : 'Draft',
        questions: data.questions.map((question, index) => ({
          ...question,
          displayOrder: index + 1,
          options: question.options?.map((option, optionIndex) => ({
            ...option,
            displayOrder: optionIndex + 1,
          })) || [],
        })),
      };

      const result = await createSurvey(surveyData);
      navigate(`/admin/surveys/${result.id}`);
    } catch (err) {
      setError('Failed to create survey. Please try again.');
      console.error('Error creating survey:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const addQuestion = () => {
    appendQuestion({
      questionText: '',
      questionType: QuestionType.TEXT,
      isRequired: false,
      options: [],
    });
  };

  const needsOptions = (questionType: string) => {
    return questionType === QuestionType.MULTIPLE_CHOICE || 
           questionType === QuestionType.CHECKBOX || 
           questionType === QuestionType.DROPDOWN;
  };

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

      <div className="bg-white shadow rounded-lg">
        <div className="px-6 py-4 border-b border-gray-200">
          <h1 className="text-xl font-semibold text-gray-900">Create New Survey</h1>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-6">
          {/* Survey Basic Info */}
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
            <div className="sm:col-span-2">
              <label htmlFor="title" className="block text-sm font-medium text-gray-700">
                Survey Title
              </label>
              <input
                {...register('title')}
                type="text"
                id="title"
                className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                placeholder="Enter survey title"
              />
              {errors.title && (
                <p className="mt-1 text-sm text-red-600">{errors.title.message}</p>
              )}
            </div>

            <div className="sm:col-span-2">
              <label htmlFor="description" className="block text-sm font-medium text-gray-700">
                Description
              </label>
              <textarea
                {...register('description')}
                id="description"
                rows={3}
                className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                placeholder="Enter survey description"
              />
              {errors.description && (
                <p className="mt-1 text-sm text-red-600">{errors.description.message}</p>
              )}
            </div>

            <div>
              <label htmlFor="startDate" className="block text-sm font-medium text-gray-700">
                Start Date (Optional)
              </label>
              <input
                {...register('startDate')}
                type="datetime-local"
                id="startDate"
                className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
              />
            </div>

            <div>
              <label htmlFor="endDate" className="block text-sm font-medium text-gray-700">
                End Date (Optional)
              </label>
              <input
                {...register('endDate')}
                type="datetime-local"
                id="endDate"
                className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
              />
            </div>
          </div>

          {/* Questions */}
          <div className="space-y-6">
            <div className="flex items-center justify-between">
              <h3 className="text-lg font-medium text-gray-900">Questions</h3>
              <button
                type="button"
                onClick={addQuestion}
                className="inline-flex items-center px-3 py-2 border border-transparent text-sm leading-4 font-medium rounded-md text-blue-700 bg-blue-100 hover:bg-blue-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              >
                <Plus className="mr-1 h-4 w-4" />
                Add Question
              </button>
            </div>

            {questions.map((field, index) => {
              const questionType = watch(`questions.${index}.questionType`);
              
              return (
                <div key={field.id} className="bg-gray-50 p-4 rounded-lg space-y-4">
                  <div className="flex items-center justify-between">
                    <h4 className="text-md font-medium text-gray-900">Question {index + 1}</h4>
                    {questions.length > 1 && (
                      <button
                        type="button"
                        onClick={() => removeQuestion(index)}
                        className="text-red-600 hover:text-red-800"
                      >
                        <Trash2 className="h-4 w-4" />
                      </button>
                    )}
                  </div>

                  <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                    <div className="sm:col-span-2">
                      <label className="block text-sm font-medium text-gray-700">
                        Question Text
                      </label>
                      <input
                        {...register(`questions.${index}.questionText`)}
                        type="text"
                        className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                        placeholder="Enter your question"
                      />
                      {errors.questions?.[index]?.questionText && (
                        <p className="mt-1 text-sm text-red-600">
                          {errors.questions[index]?.questionText?.message}
                        </p>
                      )}
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-gray-700">
                        Question Type
                      </label>
                      <select
                        {...register(`questions.${index}.questionType`)}
                        className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                      >
                        {questionTypes.map((type) => (
                          <option key={type.value} value={type.value}>
                            {type.label}
                          </option>
                        ))}
                      </select>
                    </div>

                    <div className="flex items-center">
                      <input
                        {...register(`questions.${index}.isRequired`)}
                        type="checkbox"
                        className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                      />
                      <label className="ml-2 block text-sm text-gray-900">
                        Required
                      </label>
                    </div>
                  </div>

                  {needsOptions(questionType) && (
                    <QuestionOptions questionIndex={index} control={control} register={register} />
                  )}
                </div>
              );
            })}

            {errors.questions && (
              <p className="text-sm text-red-600">At least one question is required</p>
            )}
          </div>

          {error && (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
              {error}
            </div>
          )}

          <div className="flex justify-end space-x-3">
            <button
              type="button"
              onClick={() => navigate('/admin/surveys')}
              className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
            >
              Cancel
            </button>
            <button
              type="submit"
              onClick={() => setPublishOnCreate(false)}
              disabled={isLoading}
              className="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isLoading && !publishOnCreate ? (
                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-gray-700 mr-2"></div>
              ) : (
                <Save className="mr-2 h-4 w-4" />
              )}
              Save as Draft
            </button>
            <button
              type="submit"
              onClick={() => setPublishOnCreate(true)}
              disabled={isLoading}
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isLoading && publishOnCreate ? (
                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
              ) : (
                <Save className="mr-2 h-4 w-4" />
              )}
              Publish Survey
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

// Question Options Component
interface QuestionOptionsProps {
  questionIndex: number;
  control: any;
  register: any;
}

const QuestionOptions: React.FC<QuestionOptionsProps> = ({ questionIndex, control, register }) => {
  const { fields: options, append: appendOption, remove: removeOption } = useFieldArray({
    control,
    name: `questions.${questionIndex}.options`,
  });

  const addOption = () => {
    appendOption({ optionText: '' });
  };

  return (
    <div className="space-y-2">
      <label className="block text-sm font-medium text-gray-700">Options</label>
      {options.map((field, optionIndex) => (
        <div key={field.id} className="flex items-center space-x-2">
          <input
            {...register(`questions.${questionIndex}.options.${optionIndex}.optionText`)}
            type="text"
            className="flex-1 border border-gray-300 rounded-md shadow-sm py-1 px-2 focus:outline-none focus:ring-blue-500 focus:border-blue-500 text-sm"
            placeholder={`Option ${optionIndex + 1}`}
          />
          {options.length > 1 && (
            <button
              type="button"
              onClick={() => removeOption(optionIndex)}
              className="text-red-600 hover:text-red-800"
            >
              <Trash2 className="h-4 w-4" />
            </button>
          )}
        </div>
      ))}
      <button
        type="button"
        onClick={addOption}
        className="text-sm text-blue-600 hover:text-blue-800"
      >
        + Add Option
      </button>
    </div>
  );
};