import { useEffect, useCallback, useRef } from 'react';
import { memeQuotesApi } from '../api/memes';
import { useNotifications } from '../contexts/NotificationContext';

interface UseMemeNotificationsOptions {
  enabled?: boolean;
  minInterval?: number; // minimum time between notifications in milliseconds
  maxInterval?: number; // maximum time between notifications in milliseconds
}

export const useMemeNotifications = (options: UseMemeNotificationsOptions = {}) => {
  const {
    enabled = true,
    minInterval = 2 * 60 * 1000, // 2 minutes
    maxInterval = 10 * 60 * 1000, // 10 minutes
  } = options;

  const { addNotification } = useNotifications();
  const intervalRef = useRef<number | null>(null);
  const isEnabledRef = useRef(enabled);

  // Update the enabled state
  useEffect(() => {
    isEnabledRef.current = enabled;
  }, [enabled]);

  const showMemeQuote = useCallback(async () => {
    if (!isEnabledRef.current) return;

    try {
      const quote = await memeQuotesApi.getRandomMemeQuote();
      
      addNotification({
        type: 'meme',
        title: 'Daily Inspiration 🎯',
        message: `"${quote.content}" - ${quote.author}`,
        duration: 8000, // Show meme quotes a bit longer
        autoClose: true,
      });
    } catch (error) {
      console.error('Failed to show meme quote:', error);
    }
  }, [addNotification]);

  const scheduleNextNotification = useCallback(() => {
    if (intervalRef.current) {
      clearTimeout(intervalRef.current);
    }

    if (!isEnabledRef.current) return;

    // Random interval between min and max
    const randomInterval = Math.random() * (maxInterval - minInterval) + minInterval;
    
    intervalRef.current = setTimeout(() => {
      showMemeQuote();
      scheduleNextNotification(); // Schedule the next one
    }, randomInterval);
  }, [showMemeQuote, minInterval, maxInterval]);

  // Start the notification cycle
  useEffect(() => {
    if (enabled) {
      // Show first notification after a short delay
      const initialDelay = 30 * 1000; // 30 seconds after component mounts
      setTimeout(() => {
        showMemeQuote();
        scheduleNextNotification();
      }, initialDelay);
    }

    return () => {
      if (intervalRef.current) {
        clearTimeout(intervalRef.current);
      }
    };
  }, [enabled, showMemeQuote, scheduleNextNotification]);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      if (intervalRef.current) {
        clearTimeout(intervalRef.current);
      }
    };
  }, []);

  return {
    showMemeQuote,
  };
};