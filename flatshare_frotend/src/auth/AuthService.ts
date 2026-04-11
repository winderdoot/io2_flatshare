import { API_URL } from "../config";
import { User } from "../models/user";

export const authService = {
  login: async (email: string, password: string) => {
    const createdSession = await fetch(`${API_URL}/api/v1/sessions`, {
      method: "POST",
      body: JSON.stringify({ email, password }),
      headers: {
        "Content-Type": "application/json",
        "Accept": "application/json",
      },
    });

    const createdSessionData = await createdSession.json();
    const token = createdSessionData.token;

    const userId = await fetch(`${API_URL}/api/v1/sessions/${createdSessionData.sessionId}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Accept": "application/json",
        "Authorization": `Bearer ${token}`
      },
    })

    const userIdData = await userId.json();

    const user = await fetch(`${API_URL}/api/v1/users/${userIdData.userId}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Accept": "application/json",
        "Authorization": `Bearer ${token}`
      },
    });

    const userData = await user.json();

    const loggedInUser: User = {
      id: userData.id,
      firstName: userData.firstName,
      lastName: userData.lastName,
      email: userData.email,
      role: userData.role,
    };

    return {token, loggedInUser};
  },
};