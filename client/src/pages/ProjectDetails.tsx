import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Container, Card, Button, Form, Toast, ToastContainer, ListGroup, Badge } from 'react-bootstrap';
import { projectsApi, Task } from '../api/projects';

export const ProjectDetails = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [project, setProject] = useState<any>(null);
  const [tasks, setTasks] = useState<Task[]>([]);
  const [showAddForm, setShowAddForm] = useState(false);
  const [taskTitle, setTaskTitle] = useState('');
  const [taskDueDate, setTaskDueDate] = useState('');
  const [loading, setLoading] = useState(false);
  const [showToast, setShowToast] = useState(false);
  const [toastMessage, setToastMessage] = useState('');

  useEffect(() => {
    if (id) {
      loadProject();
    }
  }, [id]);

  const loadProject = async () => {
    try {
      const data = await projectsApi.getProject(id!);
      setProject(data);
      setTasks(data.tasks || []);
    } catch (error) {
      setToastMessage('Failed to load project');
      setShowToast(true);
      navigate('/');
    }
  };

  const handleCreateTask = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await projectsApi.createTask(id!, {
        title: taskTitle,
        dueDate: taskDueDate || undefined,
      });
      setTaskTitle('');
      setTaskDueDate('');
      setShowAddForm(false);
      setToastMessage('Task created successfully');
      setShowToast(true);
      loadProject();
    } catch (error: any) {
      setToastMessage(error.response?.data?.detail || 'Failed to create task');
      setShowToast(true);
    } finally {
      setLoading(false);
    }
  };

  const handleToggleComplete = async (task: Task) => {
    try {
      await projectsApi.updateTask(task.id, {
        title: task.title,
        dueDate: task.dueDate || undefined,
        isCompleted: !task.isCompleted,
      });
      loadProject();
    } catch (error: any) {
      setToastMessage(error.response?.data?.detail || 'Failed to update task');
      setShowToast(true);
    }
  };

  const handleDeleteTask = async (taskId: string) => {
    if (!confirm('Are you sure you want to delete this task?')) return;
    try {
      await projectsApi.deleteTask(taskId);
      setToastMessage('Task deleted successfully');
      setShowToast(true);
      loadProject();
    } catch (error: any) {
      setToastMessage(error.response?.data?.detail || 'Failed to delete task');
      setShowToast(true);
    }
  };

  if (!project) {
    return <Container>Loading...</Container>;
  }

  return (
    <div className="bg-light min-vh-100">
      <Container className="py-5" style={{ maxWidth: '1200px' }}>
        <div className="d-flex justify-content-between align-items-start mb-5">
          <div>
            <h1 className="display-5 fw-bold mb-2 text-primary">{project.title}</h1>
            <p className="lead text-muted mb-0">{project.description || 'No description provided'}</p>
          </div>
          <Button variant="outline-secondary" onClick={() => navigate('/')} className="px-4">
            ← Back to Projects
          </Button>
        </div>

        <Button 
          variant="success" 
          onClick={() => setShowAddForm(!showAddForm)} 
          className="mb-4"
          size="lg"
        >
          {showAddForm ? 'Cancel' : '+ Add New Task'}
        </Button>

        {showAddForm && (
          <Card className="mb-4 shadow-sm">
            <Card.Body className="p-4">
              <Card.Title className="mb-3">Create New Task</Card.Title>
              <Form onSubmit={handleCreateTask}>
                <Form.Group className="mb-3">
                  <Form.Label className="fw-semibold">Task Title</Form.Label>
                  <Form.Control
                    type="text"
                    placeholder="Enter task title"
                    value={taskTitle}
                    onChange={(e) => setTaskTitle(e.target.value)}
                    required
                    size="lg"
                  />
                </Form.Group>
                <Form.Group className="mb-3">
                  <Form.Label className="fw-semibold">Due Date</Form.Label>
                  <Form.Control
                    type="datetime-local"
                    value={taskDueDate}
                    onChange={(e) => setTaskDueDate(e.target.value)}
                    size="lg"
                  />
                </Form.Group>
                <Button variant="success" type="submit" disabled={loading} size="lg">
                  {loading ? 'Creating...' : 'Create Task'}
                </Button>
              </Form>
            </Card.Body>
          </Card>
        )}

        <Card className="shadow-sm border-0">
          <Card.Body className="p-0">
            <div className="p-4 border-bottom bg-light">
              <Card.Title className="mb-0">Tasks ({tasks.length})</Card.Title>
            </div>
            {tasks.length === 0 ? (
              <div className="p-5 text-center">
                <p className="text-muted mb-0">No tasks yet. Add your first task above.</p>
              </div>
            ) : (
                            <ListGroup variant="flush">
                {tasks.map((task) => (
                  <ListGroup.Item 
                    key={task.id} 
                    className="d-flex justify-content-between align-items-center py-3 px-4"
                    style={{
                      opacity: task.isCompleted ? 0.6 : 1,
                      backgroundColor: task.isCompleted ? '#f8f9fa' : 'white'
                    }}
                  >
                    <div className="d-flex align-items-center gap-3 flex-grow-1">
                      <Form.Check
                        type="checkbox"
                        checked={task.isCompleted}
                        onChange={() => handleToggleComplete(task)}
                        className="fs-5"
                        style={{ transform: 'scale(1.2)' }}
                      />
                      <div className="flex-grow-1">
                        <div 
                          style={{ 
                            textDecoration: task.isCompleted ? 'line-through' : 'none',
                            fontWeight: task.isCompleted ? 'normal' : '500',
                            fontSize: '1.05rem'
                          }}
                        >
                          {task.title}
                        </div>
                        {task.dueDate && (
                          <Badge bg={task.isCompleted ? 'secondary' : 'warning'} className="mt-1">
                            Due: {new Date(task.dueDate).toLocaleDateString()}
                          </Badge>
                        )}
                      </div>
                    </div>
                    <Button
                      variant="outline-danger"
                      size="sm"
                      onClick={() => handleDeleteTask(task.id)}
                    >
                      Delete
                    </Button>
                  </ListGroup.Item>
                ))}
              </ListGroup>
            )}
          </Card.Body>
        </Card>

        <ToastContainer position="top-end">
          <Toast show={showToast} onClose={() => setShowToast(false)} delay={3000} autohide>
            <Toast.Body>{toastMessage}</Toast.Body>
          </Toast>
        </ToastContainer>
      </Container>
    </div>
  );
};
