export const authService = {
  login: async (email: string, password: string) => {
    if (email === "test@pl.pl" && password === "pass123")
    {        
        return new Promise<string>((resolve, _) => {
            setTimeout(() => {
                resolve("token123");
            }, 2000);
        });
    }
        
    return new Promise<never>((_, reject) => {
        setTimeout(() => {
            reject("Invalid email or password");
        }, 2000);
    });
  },
};