import {User} from "../../app/models/user";
import {createAsyncThunk, createSlice, isAnyOf} from "@reduxjs/toolkit";
import {FieldValues} from "react-hook-form";
import agent from "../../app/api/agent";
import {history} from "../../index";
import {toast} from "react-toastify";
import {setBasket} from "../basket/basketSlice";

interface AccountState {
    user: User | null
}
const initialState: AccountState = {
    user: null
}

export const sighInUser = createAsyncThunk<User, FieldValues>(
    'account/signInUser',
    async (data, thunkApi) => {
        try {
            const userDto = await agent.Account.login(data);
            const {basket,...user} = userDto;
            if(basket) thunkApi.dispatch(setBasket(basket));
            localStorage.setItem('user', JSON.stringify(user));
            return user;
        } catch (error: any) {
            return thunkApi.rejectWithValue(({error: error.data}))
        }
    }
)

export const fetchCurrentUser = createAsyncThunk<User>(
    'account/fetchCurrentUser',
    async (_, thunkApi) => {
        thunkApi.dispatch(setUser(JSON.parse(localStorage.getItem('user')!)));
        try {
            const userDto = await agent.Account.currentUser();
            const {basket,...user} = userDto;
            if(basket) thunkApi.dispatch(setBasket(basket));
            localStorage.setItem('user', JSON.stringify(user));
            return user;
        } catch (error: any) {
            return thunkApi.rejectWithValue(({error: error.data}))
        }
    },
    {
        condition: () => {
            if(!localStorage.getItem('user')) return false;
        }
    }
)
export const accountSlice = createSlice({
    name: 'account',
    initialState,
    reducers: {
        signOut: (state) => {
            state.user = null;
            localStorage.removeItem('user');
            history.push('/');
        },
        setUser: (state,action) => {
            state.user = action.payload;
        }
    },
    extraReducers: (builder => {
        builder.addCase(fetchCurrentUser.rejected,(state) => {
            state.user = null;
            localStorage.removeItem('user');
            toast.error('Session expired');
            history.push('/');
        })
        builder.addMatcher(isAnyOf(sighInUser.fulfilled,fetchCurrentUser.fulfilled),(state,action) => {
            state.user = action.payload
        });
        builder.addMatcher(isAnyOf(sighInUser.rejected),(state,action) => {
            throw action.payload;
        });
    })
})
export const {signOut, setUser} = accountSlice.actions;
