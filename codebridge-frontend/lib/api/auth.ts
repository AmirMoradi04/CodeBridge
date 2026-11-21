import { api } from "@/lib/api/axios";
import { Role, type AuthResponse, type LoginInput, type RegisterInput } from "@/types/auth";

export const authApi = {
  login: (input: LoginInput) =>
    api.post<AuthResponse>("/auth/login", input).then((response) => response.data),

  register: (input: RegisterInput) => {
    // ثبت‌نام کارجو فایل رزومه دارد و باید multipart ارسال شود
    if (input.userRole === Role.JobSeeker) {
      const formData = new FormData();

      formData.append("firstName", input.firstName);
      formData.append("lastName", input.lastName);
      formData.append("email", input.email);
      formData.append("password", input.password);
      formData.append("userRole", String(input.userRole));

      if (input.referralCode) formData.append("referralCode", input.referralCode);
      if (input.resumeFile) formData.append("resumeFile", input.resumeFile);

      return api
        .post<AuthResponse>("/auth/register/jobseeker", formData, {
          headers: { "Content-Type": "multipart/form-data" }
        })
        .then((response) => response.data);
    }

    return api.post<AuthResponse>("/auth/register", input).then((response) => response.data);
  },

  refresh: (refreshToken: string) =>
    api
      .post<AuthResponse>("/auth/refresh", { refreshToken })
      .then((response) => response.data),

  logout: (refreshToken: string) =>
    api.post("/auth/logout", { refreshToken }).then((response) => response.data)
};
