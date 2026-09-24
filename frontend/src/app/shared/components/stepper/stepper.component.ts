import { Component, input, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface InfoPasoProceso {
  numero: number;
  nombre: string;
  etiquetaCorta: string;
  descripcion: string;
  queSignifica: string;
  accionEstudiante: string;
  proximoPaso: string;
  estadoBadge: string;
  badgeTipo: 'success' | 'warning' | 'info' | 'danger';
}

@Component({
  selector: 'app-stepper',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './stepper.component.html',
  styleUrls: ['./stepper.component.css'],
})
export class StepperComponent {
  estadoNombre = input<string>('Pendiente de Postulación');
  etapaActual = input<number>(1);
  modalidadNombre = input<string>('Por seleccionar');
  tienePostulacion = input<boolean>(false);
  totalRequisitos = input<number>(0);
  aprobadosRequisitos = input<number>(0);
  pendientesRequisitos = input<number>(0);
  tieneRequisitosFinalesPendientes = input<boolean>(false);

  // Paso que el usuario está consultando activamente (por defecto el actual)
  pasoConsultado = signal<number | null>(null);

  esRechazado = computed(() => {
    const est = (this.estadoNombre() || '').toUpperCase();
    return est.includes('RECHAZ') || est.includes('NEGAD');
  });

  esEtapaAprobada = computed(() => !this.esRechazado() && this.etapaActual() >= 3);

  pasoActivoVisual = computed(() => {
    return this.pasoConsultado() ?? (this.esRechazado() ? 2 : this.etapaActual());
  });

  seleccionarPaso(paso: number): void {
    if (this.pasoConsultado() === paso) {
      this.pasoConsultado.set(null); // Regresar al paso actual
    } else {
      this.pasoConsultado.set(paso);
    }
  }

  // Información detallada y dinámica de cada fase del proceso de titulación
  infoPasoActivo = computed<InfoPasoProceso>(() => {
    const paso = this.pasoActivoVisual();
    const modalidad = this.modalidadNombre() || 'Modalidad de Titulación';
    const total = this.totalRequisitos();
    const aprobados = this.aprobadosRequisitos();
    const pendientes = this.pendientesRequisitos();
    const tieneFinales = this.tieneRequisitosFinalesPendientes();
    const rechazo = this.esRechazado();

    switch (paso) {
      case 1:
        return {
          numero: 1,
          nombre: 'Postulación y Registro de Expediente',
          etiquetaCorta: 'Postulación',
          descripcion: 'Registro de la solicitud formal de titulación académica dentro del período de la cohorte activa.',
          queSignifica: this.tienePostulacion()
            ? 'Su postulación está registrada oficialmente en secretaría académica bajo la modalidad elegida.'
            : 'Debe seleccionar una modalidad de titulación habilitada para su carrera y enviar la postulación.',
          accionEstudiante: this.tienePostulacion()
            ? 'Expediente generado. Continúe con la carga de certificados requeridos.'
            : 'Seleccione su modalidad abajo y presione "Confirmar y Enviar Mi Postulación".',
          proximoPaso: 'Revisión y validación de requisitos por parte de docentes revisores y secretaría.',
          estadoBadge: this.tienePostulacion() ? 'Postulación Enviada' : 'Pendiente de Envío',
          badgeTipo: this.tienePostulacion() ? 'success' : 'warning',
        };

      case 2:
        return {
          numero: 2,
          nombre: 'Validación de Requisitos y Documentación',
          etiquetaCorta: 'Requisitos',
          descripcion: 'Revisión académica de certificados (Inglés, Vinculación, Prácticas Preprofesionales, Financiero).',
          queSignifica: rechazo
            ? 'Su expediente registra un dictamen de rechazo o inconsistencias en los requisitos presentados.'
            : aprobados === total && total > 0
              ? `El 100% de los requisitos (${aprobados}/${total}) han sido validados exitosamente.`
              : tieneFinales
                ? `Registra ${pendientes} documento(s) pendiente(s). Los requisitos iniciales permiten admitirle a la cohorte, pero los requisitos finales (ej. Suficiencia de Inglés) deben completarse antes de culminar el proceso.`
                : `Se encuentran validados ${aprobados} de ${total} requisitos por los docentes responsables.`,
          accionEstudiante: rechazo
            ? 'Revise las observaciones académicas para solventar inquietudes con secretaría.'
            : pendientes > 0
              ? 'Cargue los archivos PDF pendientes en la sección de Requisitos Académicos.'
              : 'Requisitos listos. Su expediente avanzará a la fase de desarrollo.',
          proximoPaso: 'Admisión formal y asignación de cronograma de desarrollo en su carrera.',
          estadoBadge: rechazo
            ? 'Expediente Rechazado'
            : aprobados === total && total > 0
              ? 'Todos Aprobados'
              : 'En Validación Docente',
          badgeTipo: rechazo ? 'danger' : aprobados === total && total > 0 ? 'success' : 'warning',
        };

      case 3:
        return {
          numero: 3,
          nombre: `Desarrollo de la Modalidad: ${modalidad}`,
          etiquetaCorta: 'Desarrollo',
          descripcion: `Ejecución académica del proceso de graduación en la opción seleccionada (${modalidad}).`,
          queSignifica: this.etapaActual() >= 3
            ? `Usted se encuentra admitido en ${modalidad}. Puede desarrollar su trabajo o prepararse académicamente conforme al cronograma.`
            : 'Esta fase se habilita formalmente una vez aprobada la postulación inicial.',
          accionEstudiante: modalidad.toLowerCase().includes('complexivo')
            ? 'Asista a las tutorías y repasos preparatorios para los componentes teórico y práctico.'
            : 'Coordine con su docente tutor asignado el plan de trabajo y cronograma de entregas.',
          proximoPaso: 'Revisión final de avances y habilitación para la evaluación o defensa.',
          estadoBadge: this.etapaActual() >= 3 ? 'En Desarrollo' : 'Por Iniciar',
          badgeTipo: this.etapaActual() >= 3 ? 'info' : 'warning',
        };

      case 4:
        return {
          numero: 4,
          nombre: 'Evaluación Académica y Tribunal de Grado',
          etiquetaCorta: 'Evaluación',
          descripcion: 'Sustentación pública de proyecto de investigación o rendición del Examen Complexivo ante tribunal.',
          queSignifica: this.etapaActual() >= 4
            ? 'Fase evaluativa activa. Su trabajo o preparación será calificada por el tribunal asignado.'
            : 'Fase posterior al desarrollo. Requiere tener el trabajo concluido y los requisitos de salida al día.',
          accionEstudiante: 'Presentarse en la fecha y hora designada para su sustentación o examen.',
          proximoPaso: 'Consolidación de calificaciones finales en el sistema institucional.',
          estadoBadge: this.etapaActual() >= 4 ? 'En Evaluación' : 'Pendiente',
          badgeTipo: this.etapaActual() >= 4 ? 'info' : 'warning',
        };

      case 5:
      default:
        return {
          numero: 5,
          nombre: 'Titulación Oficial y Emisión de Acta de Grado',
          etiquetaCorta: 'Titulación',
          descripcion: 'Incorporación formal y registro del título profesional en el sistema ISTPET y SENESCYT.',
          queSignifica: this.etapaActual() >= 5
            ? '¡Felicitaciones! Ha culminado con éxito todas las etapas académicas de su titulación.'
            : 'Etapa final. Requiere haber aprobado la evaluación y tener el 100% de requisitos culminados.',
          accionEstudiante: 'Firma de actas de grado y recepción de la documentación oficial.',
          proximoPaso: 'Registro oficial de su título en la Secretaría de Educación Superior (SENESCYT).',
          estadoBadge: this.etapaActual() >= 5 ? 'Titulado / Graduado' : 'Fase Final',
          badgeTipo: this.etapaActual() >= 5 ? 'success' : 'info',
        };
    }
  });
}
