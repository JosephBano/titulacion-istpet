/** Datos de un curso que se trasladan a un nuevo formulario, sin persistir una solicitud. */
export interface RequisicionPersonalInicial {
  cargo: string;
  numeroVacantes: number;
  origen: {
    idCohorteCarrera: number;
    carrera: string;
    cohorte: string;
    idsMallaComplexivo: number[];
  };
}
