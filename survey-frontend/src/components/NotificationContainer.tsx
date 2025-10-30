import React from 'react';
import { X, Smile, Info, CheckCircle, AlertTriangle, AlertCircle } from 'lucide-react';
import { useNotifications } from '../contexts/NotificationContext';
import type { Notification } from '../contexts/NotificationContext';

const NotificationItem: React.FC<{ notification: Notification }> = ({ notification }) => {
  const { removeNotification } = useNotifications();

  const getIcon = () => {
    switch (notification.type) {
      case 'meme':
        return <Smile className="w-5 h-5" />;
      case 'success':
        return <CheckCircle className="w-5 h-5" />;
      case 'warning':
        return <AlertTriangle className="w-5 h-5" />;
      case 'error':
        return <AlertCircle className="w-5 h-5" />;
      default:
        return <Info className="w-5 h-5" />;
    }
  };

  const getColorClasses = () => {
    switch (notification.type) {
      case 'meme':
        return 'bg-purple-50 border-purple-200 text-purple-800';
      case 'success':
        return 'bg-green-50 border-green-200 text-green-800';
      case 'warning':
        return 'bg-yellow-50 border-yellow-200 text-yellow-800';
      case 'error':
        return 'bg-red-50 border-red-200 text-red-800';
      default:
        return 'bg-blue-50 border-blue-200 text-blue-800';
    }
  };

  return (
    <div
      className={`
        relative p-4 mb-3 rounded-lg border shadow-lg transform transition-all duration-300 ease-in-out
        hover:scale-105 hover:shadow-xl max-w-sm
        ${getColorClasses()}
      `}
    >
      <div className="flex items-start">
        <div className="flex-shrink-0 mr-3">
          {getIcon()}
        </div>
        <div className="flex-1 min-w-0">
          <p className="text-sm font-medium mb-1">
            {notification.title}
          </p>
          <p className="text-sm opacity-90">
            {notification.message}
          </p>
        </div>
        <button
          onClick={() => removeNotification(notification.id)}
          className="flex-shrink-0 ml-2 p-1 rounded-full hover:bg-black hover:bg-opacity-10 transition-colors"
        >
          <X className="w-4 h-4" />
        </button>
      </div>
    </div>
  );
};

export const NotificationContainer: React.FC = () => {
  const { notifications } = useNotifications();

  if (notifications.length === 0) {
    return null;
  }

  return (
    <div className="fixed top-4 right-4 z-50 space-y-2">
      {notifications.map((notification) => (
        <NotificationItem key={notification.id} notification={notification} />
      ))}
    </div>
  );
};