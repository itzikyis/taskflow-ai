import { useState } from 'react';
import { useProjects, useDeleteProject } from '../hooks/useProjects';
import { ProjectCard } from './ProjectCard';
import { CreateProjectForm } from './CreateProjectForm';
import { BoardListPage } from '@/features/boards/components/BoardListPage';

export function ProjectListPage() {
  const { data: projects, isLoading, isError } = useProjects();
  const deleteMutation = useDeleteProject();
  const [activeProject, setActiveProject] = useState<{ id: string; name: string } | null>(null);
  const [showCreate, setShowCreate]       = useState(false);

  if (activeProject) {
    return (
      <BoardListPage
        projectId={activeProject.id}
        projectName={activeProject.name}
        onBack={() => setActiveProject(null)}
      />
    );
  }

  if (isLoading) return (
    <div className="empty-state">
      <div className="empty-state-icon">⌛</div>
      <p className="empty-state-text">Loading projects…</p>
    </div>
  );
  if (isError) return (
    <div className="empty-state">
      <div className="empty-state-icon">⚠️</div>
      <p className="empty-state-text">Failed to load projects. Please refresh.</p>
    </div>
  );

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Projects</h1>
          <p className="page-subtitle">{projects?.length ?? 0} projects</p>
        </div>
        <button
          type="button"
          className="tf-btn tf-btn-primary"
          onClick={() => setShowCreate(true)}
        >
          + New project
        </button>
      </div>

      {(!projects || projects.length === 0) ? (
        <div className="empty-state">
          <div className="empty-state-icon">📁</div>
          <p className="empty-state-text">No projects yet. Create your first one!</p>
          <button
            type="button"
            className="tf-btn tf-btn-primary"
            onClick={() => setShowCreate(true)}
          >
            + New project
          </button>
        </div>
      ) : (
        <div className="project-grid">
          {projects.map(project => (
            <ProjectCard
              key={project.id}
              project={project}
              onDelete={() => deleteMutation.mutate(project.id)}
              onOpenBoards={() => setActiveProject({ id: project.id, name: project.name })}
            />
          ))}
        </div>
      )}

      {showCreate && <CreateProjectForm onClose={() => setShowCreate(false)} />}
    </div>
  );
}
