import { Navigate } from "react-router-dom";
import { authService } from "../services/AuthService";

const AdminRoute = ({ children }) => {
  const user = authService.getUserFromToken();

  if (!user || user.role !== "admin") {
    return <Navigate to="/home" replace />;
  }

  return children;
};

export default AdminRoute;
