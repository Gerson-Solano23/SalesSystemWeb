export interface User {
    idUser:number,
    fullName:string,
    email:string,
    idRol:number,
    rolDescription:string,
    // Only sent when creating a user or changing the password; the API never returns it.
    password?:string,
    status:number
}
