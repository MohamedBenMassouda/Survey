import axios from 'axios';

interface MemeQuote {
  id: string;
  author: string;
  content: string;
  tags: string[];
  authorSlug: string;
  length: number;
  dateAdded: string;
  dateModified: string;
}

// Using Quotable API - a free quotes API
const QUOTES_API_URL = 'https://api.quotable.io';

export const memeQuotesApi = {
  // Get a random funny/motivational quote
  getRandomMemeQuote: async (): Promise<MemeQuote> => {
    try {
      // Fetching quotes with funny, motivational, or technology tags
      const response = await axios.get(`${QUOTES_API_URL}/random`, {
        params: {
          tags: 'motivational|technology|wisdom|success',
          maxLength: 150, // Keep quotes reasonably short for notifications
        }
      });
      return response.data;
    } catch (error) {
      console.error('Failed to fetch meme quote:', error);
      // Return a fallback quote if API fails
      return {
        id: 'fallback',
        author: 'Anonymous',
        content: "Don't let yesterday take up too much of today! 🚀",
        tags: ['motivational'],
        authorSlug: 'anonymous',
        length: 45,
        dateAdded: new Date().toISOString(),
        dateModified: new Date().toISOString(),
      };
    }
  },

  // Get multiple random quotes for variety
  getMultipleRandomQuotes: async (count: number = 5): Promise<MemeQuote[]> => {
    try {
      const promises = Array.from({ length: count }, () => 
        memeQuotesApi.getRandomMemeQuote()
      );
      return await Promise.all(promises);
    } catch (error) {
      console.error('Failed to fetch multiple quotes:', error);
      return [];
    }
  }
};