import { Navigate } from "react-router-dom";
import { authService } from "../services/AuthService";

const UserRoute = ({ children }) => {
  const user = authService.getUserFromToken();

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  // Optional: block admins from user area
  if (user.role === "admin") {
    return <Navigate to="/admin" replace />;
  }

  return children;
};

export default UserRoute;
