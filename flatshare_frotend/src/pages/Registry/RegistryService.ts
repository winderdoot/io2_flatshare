import { API_URL } from "../../config";

export const registryService = {
    register: async ( firstName: string, lastName: string, email: string, password: string, role: string) => {
        const res = await fetch(`${API_URL}/api/v1/users`, {
              method: "POST",
              body: JSON.stringify({ firstName, lastName, email, password, role }),
              headers: {
                "Content-Type": "application/json",
                "Accept": "application/json",
              },
            });
        
        
        const data = await res.json();
        return data;
    }
};