import { Session } from './session';

export interface LoginResponse {
    token: string,
    session: Session
}
