export interface LoginRequest {
  usernameOrEmail: string;
  password: string;
  systemCode?: string;
}

export interface RbacModuloPermissions {
  idModulo: number;
  nombreModulo: string;
  operaciones: string[];
}

export interface UserPermissions {
  idUsuario: number;
  nombre: string;
  emailInstitucional: string;
  idSigafi: string;
  tablaSigafi: string;
  roles: string[];
  modulos: RbacModuloPermissions[];
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  userInfo: UserPermissions;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface ValidarDocumentoRecuperacionRequest {
  numeroDocumento: string;
}

export interface ValidarDocumentoRecuperacionResponse {
  valido: boolean;
  mensaje: string;
  nombreCompleto?: string;
  requiereFechaNacimiento?: boolean;
}

export interface ValidarFechaNacimientoRequest {
  numeroDocumento: string;
  fechaNacimiento: string;
}

export interface ValidarFechaNacimientoResponse {
  valido: boolean;
  mensaje: string;
  token?: string;
}

export interface RestablecerContrasenaRequest {
  numeroDocumento: string;
  token: string;
  nuevaContrasenia: string;
  confirmarContrasenia: string;
}

export interface RestablecerContrasenaResponse {
  exito: boolean;
  mensaje: string;
}
