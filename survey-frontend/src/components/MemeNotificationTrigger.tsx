import React from 'react';
import { useMemeNotifications } from '../hooks/useMemeNotifications';

export const MemeNotificationTrigger: React.FC = () => {
  // Initialize meme notifications when this component mounts
  useMemeNotifications({
    enabled: true,
    minInterval: 3 * 60 * 1000, // 3 minutes minimum
    maxInterval: 8 * 60 * 1000, // 8 minutes maximum
  });

  // This component doesn't render anything visible
  return null;
};