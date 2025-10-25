import { useState } from 'react';
import { Container, Row, Col, Card, Button, Form, Toast, ToastContainer, Spinner, Alert, Badge } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { schedulerApi, SchedulerTask, SchedulerOrderResponse } from '../api/scheduler';

export const Scheduler = () => {
  const [tasks, setTasks] = useState<SchedulerTask[]>([
    { title: '', estimatedHours: undefined, dueDate: '', dependencies: [] }
  ]);
  const [strategy, setStrategy] = useState<'DepsDueSjf' | 'DepsDueFifo' | 'DepsOnly'>('DepsDueSjf');
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<SchedulerOrderResponse | null>(null);
  const [showToast, setShowToast] = useState(false);
  const [toastMessage, setToastMessage] = useState('');
  const [toastVariant, setToastVariant] = useState<'success' | 'danger'>('success');
  const { logout } = useAuth();
  const navigate = useNavigate();

  const addTask = () => {
    setTasks([...tasks, { title: '', estimatedHours: undefined, dueDate: '', dependencies: [] }]);
  };

  const removeTask = (index: number) => {
    if (tasks.length > 1) {
      const newTasks = tasks.filter((_, i) => i !== index);
      setTasks(newTasks);
    }
  };

  const updateTask = (index: number, field: keyof SchedulerTask, value: any) => {
    const newTasks = [...tasks];
    if (field === 'dependencies') {
      newTasks[index][field] = value.split(',').map((dep: string) => dep.trim()).filter((dep: string) => dep);
    } else {
      newTasks[index][field] = value;
    }
    setTasks(newTasks);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setResult(null);

    try {
      // Validate tasks
      const validTasks = tasks.filter(task => task.title.trim());
      if (validTasks.length === 0) {
        throw new Error('At least one task with a title is required');
      }

      // Convert date strings to proper format
      const formattedTasks = validTasks.map(task => ({
        ...task,
        dueDate: task.dueDate ? new Date(task.dueDate).toISOString().split('T')[0] : undefined,
        estimatedHours: task.estimatedHours || undefined
      }));

      const response = await schedulerApi.getRecommendedOrder({
        tasks: formattedTasks,
        strategy
      });

      setResult(response);
      setToastMessage('Schedule generated successfully!');
      setToastVariant('success');
      setShowToast(true);
    } catch (error: any) {
      const errorMessage = error.response?.data?.detail || error.message || 'Failed to generate schedule';
      setToastMessage(errorMessage);
      setToastVariant('danger');
      setShowToast(true);
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = async () => {
    await logout();
  };

  return (
    <div className="bg-light min-vh-100">
      <Container className="py-5" style={{ maxWidth: '1200px' }}>
        <div className="d-flex justify-content-between align-items-center mb-5">
          <div>
            <h1 className="display-5 fw-bold mb-2 text-primary">Smart Scheduler</h1>
            <p className="lead text-muted">Generate optimal task order based on dependencies and constraints</p>
          </div>
          <div className="d-flex gap-2">
            <Button variant="outline-secondary" onClick={() => navigate('/')} className="px-4">
              ← Back to Projects
            </Button>
            <Button variant="outline-danger" onClick={handleLogout} className="px-4">
              Logout
            </Button>
          </div>
        </div>

        <Row>
          <Col lg={6}>
            <Card className="shadow-sm">
              <Card.Header className="bg-primary text-white">
                <h4 className="mb-0">Task Configuration</h4>
              </Card.Header>
              <Card.Body>
                <Form onSubmit={handleSubmit}>
                  <Form.Group className="mb-3">
                    <Form.Label className="fw-semibold">Scheduling Strategy</Form.Label>
                    <Form.Select
                      value={strategy}
                      onChange={(e) => setStrategy(e.target.value as any)}
                      size="lg"
                    >
                      <option value="DepsDueSjf">Dependencies + Due Date + Shortest Job First</option>
                      <option value="DepsDueFifo">Dependencies + Due Date + First In First Out</option>
                      <option value="DepsOnly">Dependencies Only</option>
                    </Form.Select>
                  </Form.Group>

                  <div className="mb-3">
                    <div className="d-flex justify-content-between align-items-center mb-2">
                      <Form.Label className="fw-semibold mb-0">Tasks</Form.Label>
                      <Button variant="outline-primary" size="sm" onClick={addTask}>
                        + Add Task
                      </Button>
                    </div>
                    
                    {tasks.map((task, index) => (
                      <Card key={index} className="mb-3 border">
                        <Card.Body className="p-3">
                          <div className="d-flex justify-content-between align-items-center mb-2">
                            <h6 className="mb-0">Task {index + 1}</h6>
                            {tasks.length > 1 && (
                              <Button
                                variant="outline-danger"
                                size="sm"
                                onClick={() => removeTask(index)}
                              >
                                Remove
                              </Button>
                            )}
                          </div>
                          
                          <Form.Group className="mb-2">
                            <Form.Label>Title *</Form.Label>
                            <Form.Control
                              type="text"
                              placeholder="Enter task title"
                              value={task.title}
                              onChange={(e) => updateTask(index, 'title', e.target.value)}
                              required
                            />
                          </Form.Group>
                          
                          <Row>
                            <Col md={6}>
                              <Form.Group className="mb-2">
                                <Form.Label>Estimated Hours</Form.Label>
                                <Form.Control
                                  type="number"
                                  placeholder="e.g., 5"
                                  value={task.estimatedHours || ''}
                                  onChange={(e) => updateTask(index, 'estimatedHours', e.target.value ? parseFloat(e.target.value) : undefined)}
                                  min="0"
                                  step="0.5"
                                />
                              </Form.Group>
                            </Col>
                            <Col md={6}>
                              <Form.Group className="mb-2">
                                <Form.Label>Due Date</Form.Label>
                                <Form.Control
                                  type="date"
                                  value={task.dueDate || ''}
                                  onChange={(e) => updateTask(index, 'dueDate', e.target.value)}
                                />
                              </Form.Group>
                            </Col>
                          </Row>
                          
                          <Form.Group>
                            <Form.Label>Dependencies (comma-separated task titles)</Form.Label>
                            <Form.Control
                              type="text"
                              placeholder="e.g., Task A, Task B"
                              value={task.dependencies.join(', ')}
                              onChange={(e) => updateTask(index, 'dependencies', e.target.value)}
                            />
                          </Form.Group>
                        </Card.Body>
                      </Card>
                    ))}
                  </div>

                  <Button
                    variant="success"
                    type="submit"
                    disabled={loading}
                    size="lg"
                    className="w-100"
                  >
                    {loading ? (
                      <>
                        <Spinner animation="border" size="sm" className="me-2" />
                        Generating...
                      </>
                    ) : (
                      'Generate Schedule'
                    )}
                  </Button>
                </Form>
              </Card.Body>
            </Card>
          </Col>

          <Col lg={6}>
            <Card className="shadow-sm">
              <Card.Header className="bg-success text-white">
                <h4 className="mb-0">Recommended Order</h4>
              </Card.Header>
              <Card.Body>
                {result ? (
                  <div>
                    <Alert variant="success" className="mb-3">
                      <strong>Strategy Used:</strong> {result.strategyUsed}
                    </Alert>
                    
                    <div className="mb-3">
                      <h6 className="fw-semibold">Execution Order:</h6>
                      <div className="list-group">
                        {result.recommendedOrder.map((taskTitle, index) => (
                          <div
                            key={index}
                            className="list-group-item d-flex justify-content-between align-items-center"
                          >
                            <div className="d-flex align-items-center">
                              <Badge bg="primary" className="me-2">
                                {index + 1}
                              </Badge>
                              <span className="fw-medium">{taskTitle}</span>
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  </div>
                ) : (
                  <div className="text-center py-5">
                    <div className="text-muted">
                      <i className="bi bi-calendar-check display-1"></i>
                      <h5 className="mt-3">No schedule generated yet</h5>
                      <p>Configure your tasks and click "Generate Schedule" to see the recommended order.</p>
                    </div>
                  </div>
                )}
              </Card.Body>
            </Card>
          </Col>
        </Row>

        <ToastContainer position="top-end">
          <Toast show={showToast} onClose={() => setShowToast(false)} delay={5000} autohide>
            <Toast.Body className={toastVariant === 'success' ? 'text-success' : 'text-danger'}>
              {toastMessage}
            </Toast.Body>
          </Toast>
        </ToastContainer>
      </Container>
    </div>
  );
};
