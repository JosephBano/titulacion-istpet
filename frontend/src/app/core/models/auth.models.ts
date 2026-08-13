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
