import { useState } from 'react';
import { Modal, Button, Form, Toast, ToastContainer } from 'react-bootstrap';
import { feedbackApi } from '../api/feedback';

interface FeedbackModalProps {
  show: boolean;
  onHide: () => void;
}

export const FeedbackModal = ({ show, onHide }: FeedbackModalProps) => {
  const [rating, setRating] = useState(0);
  const [comment, setComment] = useState('');
  const [loading, setLoading] = useState(false);
  const [showToast, setShowToast] = useState(false);
  const [toastMessage, setToastMessage] = useState('');
  const [submitted, setSubmitted] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (rating === 0) {
      setToastMessage('Please select a rating');
      setShowToast(true);
      return;
    }

    setLoading(true);
    try {
      await feedbackApi.createFeedback({
        rating,
        comment: comment || undefined
      });
      setSubmitted(true);
      setToastMessage('Thank you for your feedback!');
      setShowToast(true);
      setTimeout(() => {
        onHide();
        handleReset();
      }, 2000);
    } catch (error: any) {
      setToastMessage(error.response?.data?.detail || 'Failed to submit feedback');
      setShowToast(true);
    } finally {
      setLoading(false);
    }
  };

  const handleReset = () => {
    setRating(0);
    setComment('');
    setSubmitted(false);
  };

  const handleClose = () => {
    if (!submitted) {
      handleReset();
    }
    onHide();
  };

  return (
    <>
      <Modal show={show} onHide={handleClose} centered>
        <Modal.Header closeButton>
          <Modal.Title>Share Your Feedback</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          {!submitted ? (
            <Form onSubmit={handleSubmit}>
              <Form.Group className="mb-4">
                <Form.Label className="fw-semibold d-block mb-3">
                  How would you rate your experience?
                </Form.Label>
                <div className="d-flex justify-content-center gap-2">
                  {[1, 2, 3, 4, 5].map((star) => (
                    <Button
                      key={star}
                      variant={star <= rating ? 'warning' : 'outline-secondary'}
                      onClick={() => setRating(star)}
                      size="lg"
                      className="rounded-circle d-flex align-items-center justify-content-center p-0"
                      style={{ width: '50px', height: '50px', fontSize: '1.5rem' }}
                    >
                      ⭐
                    </Button>
                  ))}
                </div>
                <Form.Text className="text-muted d-block text-center mt-2">
                  {rating === 0 ? 'Select a rating' : `You selected ${rating} ${rating === 1 ? 'star' : 'stars'}`}
                </Form.Text>
              </Form.Group>

              <Form.Group className="mb-3">
                <Form.Label className="fw-semibold">Additional Comments (Optional)</Form.Label>
                <Form.Control
                  as="textarea"
                  rows={4}
                  placeholder="Tell us what you think..."
                  value={comment}
                  onChange={(e) => setComment(e.target.value)}
                  maxLength={1000}
                />
                <Form.Text className="text-muted">
                  {comment.length}/1000 characters
                </Form.Text>
              </Form.Group>

              <Button variant="primary" type="submit" disabled={loading || rating === 0} className="w-100" size="lg">
                {loading ? 'Submitting...' : 'Submit Feedback'}
              </Button>
            </Form>
          ) : (
            <div className="text-center py-4">
              <div className="display-4 mb-3">🎉</div>
              <h4>Thank you!</h4>
              <p className="text-muted">Your feedback has been submitted successfully.</p>
            </div>
          )}
        </Modal.Body>
      </Modal>

      <ToastContainer position="top-end">
        <Toast show={showToast} onClose={() => setShowToast(false)} delay={3000} autohide>
          <Toast.Body>{toastMessage}</Toast.Body>
        </Toast>
      </ToastContainer>
    </>
  );
};
