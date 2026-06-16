import { useState } from 'react';
import { useProjects, useDeleteProject } from '../hooks/useProjects';
import { ProjectCard } from './ProjectCard';
import { CreateProjectForm } from './CreateProjectForm';
import { BoardListPage } from '@/features/boards/components/BoardListPage';

export function ProjectListPage() {
  const { data: projects, isLoading, isError } = useProjects();
  const deleteMutation = useDeleteProject();
  const [activeProject, setActiveProject] = useState<{ id: string; name: string } | null>(null);

  if (isLoading) return <p>Loading projects…</p>;
  if (isError) return <p role="alert">Failed to load projects.</p>;

  if (activeProject) {
    return (
      <BoardListPage
        projectId={activeProject.id}
        projectName={activeProject.name}
        onBack={() => setActiveProject(null)}
      />
    );
  }

  return (
    <section>
      <h2>Projects</h2>
      <CreateProjectForm />
      {projects?.length === 0 && <p>No projects yet. Create one above!</p>}
      <ul style={{ listStyle: 'none', padding: 0 }}>
        {projects?.map((project) => (
          <li key={project.id}>
            <ProjectCard
              project={project}
              onDelete={() => deleteMutation.mutate(project.id)}
              onOpenBoards={() => setActiveProject({ id: project.id, name: project.name })}
            />
          </li>
        ))}
      </ul>
    </section>
  );
}
