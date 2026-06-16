import { useProjects, useDeleteProject } from '../hooks/useProjects';
import { ProjectCard } from './ProjectCard';
import { CreateProjectForm } from './CreateProjectForm';

export function ProjectListPage() {
  const { data: projects, isLoading, isError } = useProjects();
  const deleteMutation = useDeleteProject();

  if (isLoading) return <p>Loading projects…</p>;
  if (isError) return <p role="alert">Failed to load projects.</p>;

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
            />
          </li>
        ))}
      </ul>
    </section>
  );
}
