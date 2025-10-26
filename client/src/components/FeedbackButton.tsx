import { useState } from 'react';
import { Button } from 'react-bootstrap';
import { useAuth } from '../contexts/AuthContext';
import { FeedbackModal } from './FeedbackModal';

interface FeedbackButtonProps {
  onShow: () => void;
}

export const FeedbackButton = ({ onShow }: FeedbackButtonProps) => {
  const { isAuthenticated } = useAuth();
  const [showModal, setShowModal] = useState(false);

  // Only show feedback button for authenticated users
  if (!isAuthenticated) {
    return null;
  }

  return (
    <>
      <Button
        variant="info"
        className="position-fixed bottom-0 end-0 m-4 rounded-circle shadow-lg"
        style={{ width: '60px', height: '60px', fontSize: '1.5rem' }}
        onClick={() => {
          setShowModal(true);
          onShow();
        }}
        title="Share Your Feedback"
      >
        💬
      </Button>
      
      <FeedbackModal
        show={showModal}
        onHide={() => setShowModal(false)}
      />
    </>
  );
};
