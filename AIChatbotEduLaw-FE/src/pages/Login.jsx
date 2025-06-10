import React from "react";
import Header from "../components/HeaderLogin";
import "./Login.css";
import { Link, useNavigate } from "react-router-dom";
import { signInWithPopup } from "firebase/auth";
import { auth, googleProvider } from "../config/firebase";

function Login() {
  const navigate = useNavigate();

  const handleGoogleLogin = async () => {
    try {
      const result = await signInWithPopup(auth, googleProvider);
      // You can access the Google user info from result.user
      console.log("Google login successful:", result.user);
      // Redirect to home page after successful login
      navigate("/home");
    } catch (error) {
      console.error("Error during Google login:", error);
      // Handle error appropriately (e.g., show error message to user)
    }
  };

  return (
    <div>
      <Header />
      <div className="login">
        <h1>Welcome to EduLawAI!</h1>
        <div className="login content">
          <div className="login email">
            <input style={{}} type="text" placeholder="Enter your email" />
          </div>
          <div className="login password">
            <input type="text" placeholder="Enter your password" />
          </div>
          <div className="login button">
            <button>Login</button>
          </div>
        </div>
      </div>
      <div className="signup">
        <span>Don't have an account? </span>
        <Link to="/signup" className="signupText">
          Sign Up
        </Link>
      </div>
      <div className="login google">
        <span style={{ margin: "10px" }}>Or</span>
        <button className="googleButton" onClick={handleGoogleLogin}>
          <img
            src="https://upload.wikimedia.org/wikipedia/commons/thumb/c/c1/Google_%22G%22_logo.svg/800px-Google_%22G%22_logo.svg.png"
            alt="Google Logo"
          />
          Login with Google
        </button>
      </div>
    </div>
  );
}

export default Login;
