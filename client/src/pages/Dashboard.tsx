import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Container, Row, Col, Card, Button, Form, Toast, ToastContainer } from 'react-bootstrap';
import { useAuth } from '../contexts/AuthContext';
import { projectsApi, Project } from '../api/projects';

export const Dashboard = () => {
  const [projects, setProjects] = useState<Project[]>([]);
  const [showAddForm, setShowAddForm] = useState(false);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [loading, setLoading] = useState(false);
  const [showToast, setShowToast] = useState(false);
  const [toastMessage, setToastMessage] = useState('');
  const { logout } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    loadProjects();
  }, []);

  const loadProjects = async () => {
    try {
      const data = await projectsApi.getProjects();
      setProjects(data);
    } catch (error) {
      setToastMessage('Failed to load projects');
      setShowToast(true);
    }
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await projectsApi.createProject({ title, description });
      setTitle('');
      setDescription('');
      setShowAddForm(false);
      setToastMessage('Project created successfully');
      setShowToast(true);
      loadProjects();
    } catch (error: any) {
      setToastMessage(error.response?.data?.detail || 'Failed to create project');
      setShowToast(true);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Are you sure you want to delete this project?')) return;
    try {
      await projectsApi.deleteProject(id);
      setToastMessage('Project deleted successfully');
      setShowToast(true);
      loadProjects();
    } catch (error: any) {
      setToastMessage(error.response?.data?.detail || 'Failed to delete project');
      setShowToast(true);
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
            <h1 className="display-5 fw-bold mb-2 text-primary">My Projects</h1>
            <p className="lead text-muted">Manage your projects and tasks</p>
          </div>
          <div className="d-flex gap-2">
            <Button variant="outline-primary" onClick={() => navigate('/scheduler')} className="px-4">
              Smart Scheduler
            </Button>
            <Button variant="outline-danger" onClick={handleLogout} className="px-4">
              Logout
            </Button>
          </div>
        </div>

        <Button 
          variant="primary" 
          onClick={() => setShowAddForm(!showAddForm)} 
          className="mb-4"
          size="lg"
        >
          {showAddForm ? 'Cancel' : '+ Add New Project'}
        </Button>

        {showAddForm && (
          <Card className="mb-4 shadow-sm">
            <Card.Body className="p-4">
              <Card.Title className="mb-3">Create New Project</Card.Title>
              <Form onSubmit={handleCreate}>
                <Form.Group className="mb-3">
                  <Form.Label className="fw-semibold">Title</Form.Label>
                  <Form.Control
                    type="text"
                    placeholder="Enter project title"
                    value={title}
                    onChange={(e) => setTitle(e.target.value)}
                    required
                    minLength={3}
                    maxLength={100}
                    size="lg"
                  />
                </Form.Group>
                <Form.Group className="mb-3">
                  <Form.Label className="fw-semibold">Description</Form.Label>
                  <Form.Control
                    as="textarea"
                    rows={4}
                    placeholder="Enter project description (optional)"
                    value={description}
                    onChange={(e) => setDescription(e.target.value)}
                    maxLength={500}
                  />
                </Form.Group>
                <Button variant="success" type="submit" disabled={loading} size="lg">
                  {loading ? 'Creating...' : 'Create Project'}
                </Button>
              </Form>
            </Card.Body>
          </Card>
        )}

        {projects.length === 0 ? (
          <div className="text-center py-5">
            <h4 className="text-muted">No projects yet</h4>
            <p className="text-muted">Create your first project to get started!</p>
          </div>
        ) : (
          <Row>
            {projects.map((project) => (
              <Col key={project.id} md={6} lg={4} className="mb-4">
                <Card className="h-100 shadow-sm border-0" style={{ transition: 'transform 0.2s' }}
                  onMouseEnter={(e) => e.currentTarget.style.transform = 'translateY(-5px)'}
                  onMouseLeave={(e) => e.currentTarget.style.transform = 'translateY(0)'}
                >
                  <Card.Body className="d-flex flex-column">
                    <Card.Title className="mb-3">{project.title}</Card.Title>
                    <Card.Text className="text-muted flex-grow-1">
                      {project.description || 'No description provided'}
                    </Card.Text>
                    <div className="d-flex gap-2 mt-3">
                      <Button
                        variant="primary"
                        onClick={() => navigate(`/projects/${project.id}`)}
                        className="flex-grow-1"
                      >
                        View Project
                      </Button>
                      <Button 
                        variant="outline-danger" 
                        onClick={() => handleDelete(project.id)}
                      >
                        Delete
                      </Button>
                    </div>
                  </Card.Body>
                </Card>
              </Col>
            ))}
          </Row>
        )}

        <ToastContainer position="top-end">
          <Toast show={showToast} onClose={() => setShowToast(false)} delay={3000} autohide>
            <Toast.Body>{toastMessage}</Toast.Body>
          </Toast>
        </ToastContainer>
      </Container>
    </div>
  );
};
